using DG.Tweening;
using UnityEngine;

public class Rocket : MonoBehaviour {
    public GameObject fuelPrefab;
    public AudioClip noiseLow;
    public AudioClip noiseLowToHigh;
    public AudioClip noiseHighToLow;
    public AudioClip addToRocketClip;

    GameController gameController;
    CameraShake cameraShake;

    AudioSource audioSource;
    ParticleSystem rocketExhaust;
    BoxCollider2D rocket;
    Collider2D middle;
    Collider2D top;
    int fuelCount;

    bool ReadyForLaunch { get { return fuelCount == 5; } }

    // Use this for initialization
    void Start () {
        gameController = GameObject.FindObjectOfType<GameController>();

        audioSource = GetComponent<AudioSource>();

        rocketExhaust = this.transform.Find("Exhaust").GetComponent<ParticleSystem>();
        rocket = GetComponent<BoxCollider2D>();
        middle = GameObject.Find("Middle").GetComponent<Collider2D>();
        top = GameObject.Find("Top").GetComponent<Collider2D>();

        cameraShake = Camera.main.GetComponent<CameraShake>();
    }

    public void StartGame() {
        EndGame();

        // Drop middle part
        Util.MakeDynamic(middle);
    }

    public void EndGame() {
        StopEngines();

        // Destroy any fuel left over
        GameObject.Destroy(GameObject.FindGameObjectWithTag("Fuel"));

        // Reset rocket part positions and state
        rocket.offset = Vector3.zero;
        rocket.size = GetComponent<SpriteRenderer>().bounds.size;
        middle.transform.parent = rocket.transform;
        top.transform.parent = rocket.transform;
        middle.transform.localPosition = new Vector3(-8f, 26, 0);
        top.transform.localPosition = new Vector3(-22.5f, 26, 0);
        middle.gameObject.layer = Layers.Collectable;
        top.gameObject.layer = Layers.Collectable;
        Util.MakeKinematic(middle);
        Util.MakeKinematic(top);
    }

    private void OnTriggerEnter2D(Collider2D collider) {
        if (collider.transform == gameController.player.transform) {
            // If we collide with the rocket and it's fully fueled, launch the rocket
            if (ReadyForLaunch) {
                rocket.gameObject.layer = Layers.Rocket;
                gameController.Score(100);
                LaunchRocket();
            }
        }
        else
        if (collider == middle) {
            CompleteMiddle();
        }
        else
        if (collider == top) {
            CompleteTop();
        }
        else
        if (collider.tag == "Fuel") {
            ConsumeFuel(collider);
        }
    }

    void ConsumeFuel(Collider2D fuel) {
        PlayClip(addToRocketClip);

        fuel.enabled = false;
        fuel.transform
            .DOMove(rocket.bounds.center, 0.2f)
            .OnComplete(() =>
                fuel.transform
                    .DOScale(0, 0.1f)
                    .OnComplete(() => {
                        GameObject.Destroy(fuel.gameObject);
                    })
            );
            

        fuelCount++;
        if (ReadyForLaunch) {
            rocket.gameObject.layer = Layers.Collectable;
            StartEngines();
        }
        else {
            CreateFuel();
        }
    }

    void CreateFuel() {
        var fuel = GameObject.Instantiate(fuelPrefab);
        fuel.transform.position = new Vector3(Random.Range(-18, 4), 15);
    }

    void ExtendRocket(Collider2D part) {
        // Ignore if rocket already contains part
        if (part.transform.parent == rocket) {
            return;
        }

        var partHalfHeight = part.bounds.extents.y;
        var rocketHalfHeight = rocket.bounds.extents.y;
        var rocketCenterY = rocket.offset.y;
        var partY = rocketCenterY + rocketHalfHeight + partHalfHeight;

        // Add rocket part graphic to top of rocket
        part.gameObject.layer = Layers.Rocket;
        part.transform.parent = rocket.transform;
        part.transform.localPosition = new Vector3(0, partY, 0);
        Util.MakeKinematic(part);

        // Extend rocket box collider to encompass new part
        rocket.size += new Vector2(0, partHalfHeight * 2);
        rocket.offset += new Vector2(0, partHalfHeight);
    }

    void CompleteMiddle() {
        ExtendRocket(middle);
        PlayClip(addToRocketClip);

        // Drop top part
        Util.MakeDynamic(top);

    }

    void CompleteTop() {
        ExtendRocket(top);
        PlayClip(addToRocketClip);

        CreateFuel();
    }

    void StartEngines() {
        rocketExhaust.Play();
        PlayClip(noiseLow, loop:true);
    }

    void StopEngines() {
        rocketExhaust.Stop();
        PlayClip(null);
    }

    void LaunchRocket() {
        DOTween.To(() => cameraShake.shakeAmount, v => cameraShake.shakeAmount = v, 0.2f, 4f)
            .OnStart(() => cameraShake.enabled = true);

        gameController.player.gameObject.SetActive(false);

        rocket.gameObject.layer = Layers.Rocket;

        DOTween.Sequence()
            .Append(
                rocket.transform
                    .DOShakePosition(2, Vector2.right * 0.2f, 100, 90, false, fadeOut: false)
            )
            .Append(
                rocket.transform
                    .DOLocalMoveY(25, noiseLowToHigh.length-2)
                    .SetEase(Ease.InQuad)
            )
            .OnComplete(DescendRocket);

        PlayClip(noiseLowToHigh);
    }

    void DescendRocket() {
        gameController.RemoveAllEnemies();
        rocket.transform
            .DOLocalMoveY(-10.32f, noiseHighToLow.length)
            .SetEase(Ease.OutQuad)
            .OnComplete(LandRocket);

        PlayClip(noiseHighToLow);
    }

    void LandRocket() {
        this.fuelCount = 0;

        DOTween.To(() => cameraShake.shakeAmount, v => cameraShake.shakeAmount = v, 0f, 1f)
            .OnComplete(() => {
                cameraShake.enabled = false;
                StopEngines();
                CreateFuel();
            });

        gameController.StartNextLevel();
    }

    void PlayClip(AudioClip clip, bool loop=false) {
        audioSource.Stop();
        if (clip != null) {
            audioSource.clip = clip;
            audioSource.loop = loop;
            audioSource.Play();
        }
    }
}
