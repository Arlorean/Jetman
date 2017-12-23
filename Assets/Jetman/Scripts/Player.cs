using DG.Tweening;
using UnityEngine;

public class Player : MonoBehaviour {
    public Sprite[] walkSprites;
    public Sprite[] flySprites;
    public AudioClip pickUpClip;
    public AudioClip dropOffClip;

    GameController gameController;
    SpriteRenderer spriteRenderer;
    SpriteRenderer shadowRenderer;
    ParticleSystem exhaustEffect;
    AudioSource exhaustSound;
    Rigidbody2D body;
    Collider2D feet;
    AudioSource audioSource;

    Collider2D carrying;
    bool facingRight = true;
    bool thrusting = false;
    Vector2 direction;

    // Use this for initialization
    void Start () {
        gameController = GameObject.FindObjectOfType<GameController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        body = GetComponent<Rigidbody2D>();
        feet = GetComponent<Collider2D>();

        var exhaust = this.transform.Find("Exhaust");
        exhaustEffect = exhaust.GetComponent<ParticleSystem>();
        exhaustSound = exhaust.GetComponent<AudioSource>();

        var shadow = this.transform.Find("Shadow");
        shadowRenderer = shadow.GetComponent<SpriteRenderer>();
    }

    private void Update() {
        if (Input.touchCount != 0) {
            var touchScreenPos = Input.GetTouch(0).position;
            var touchWorldPos = Camera.main.ScreenToWorldPoint(touchScreenPos);
            var playerWorldPos = transform.position;
            direction = (touchWorldPos - playerWorldPos);
            direction.x = Mathf.Sign(direction.x);
            direction.y = Mathf.Sign(direction.y);
        }
        else {
            direction = Vector2.zero;
        }
    }

    void FixedUpdate() {
        var bottom = feet.bounds.center + new Vector3(0, -feet.bounds.extents.y, 0);
        var grounded = Physics2D.OverlapCircle(bottom, 0.2f, Layers.Mask(Layers.Ground));
        var v = body.velocity;

        var speed = 5f;

        if (direction == Vector2.zero) {
            direction = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        }

        if (direction.y == 0) {
            if (thrusting) {
                thrusting = false;
                exhaustEffect.Stop();
                exhaustSound.Stop();
            }
        }
        else {
            if (!thrusting) {
                thrusting = true;
                exhaustEffect.Play();
                exhaustSound.Play();
            }
        }
        if (direction.y < 0) {
            body.constraints |= RigidbodyConstraints2D.FreezePositionY;
        }
        else {
            body.constraints &= ~RigidbodyConstraints2D.FreezePositionY;
            body.AddForce(Vector2.up * (direction.y * speed/Time.deltaTime));
        }

        if (grounded) {
            v.x = direction.x * (speed) * 0.75f;
        }
        else {
            body.AddForce(Vector2.right * (direction.x * speed/Time.deltaTime));
        }

        // Update facingRight
        if (direction.x < 0) {
            facingRight = false;
        }
        if (direction.x > 0) {
            facingRight = true;
        }
        transform.localScale = facingRight ? new Vector3(+1, 1, 1) : new Vector3(-1, 1, 1);

        // Update sprite
        if (grounded) {
            var index = (int)(Mathf.Abs(transform.position.x*4) % walkSprites.Length);
            SetSprite(walkSprites[index]);
        }
        else {
            var index = (int)((Time.time*2) % flySprites.Length);
            SetSprite(flySprites[index]);
        }

        v.x = Mathf.Clamp(v.x, -speed, +speed);
        v.y = v.y > speed ? speed : v.y;
        body.velocity = v;

        // Don't go off the top of the screen
        var p = transform.position;
        if (p.y > 10) {
            transform.position = new Vector3(p.x, 10, p.z);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider) {
        if (collider.tag == "DropZone" && carrying != null) {
            gameController.Score(100);
            Dropoff(collider);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        var collider = collision.collider;

        if (collider.gameObject.layer == Layers.Enemy) {
            if (carrying != null) {
                carrying.gameObject.layer = Layers.Collectable;
                Dropoff(carrying);
            }
            gameController.PlayerDies();
        }
        else
        if (collider.gameObject.layer == Layers.Collectable) {
            Pickup(collider);
        }
    }

    void Pickup(Collider2D collider) {
        this.carrying = collider;
        Util.MakeKinematic(carrying);

        carrying.gameObject.layer = Layers.Rocket;
        carrying.transform.SetParent(this.transform, true);
        carrying.transform
            .DOLocalMove(Vector3.zero, 0.2f);

        PlayClip(pickUpClip);
    }

    void Dropoff(Collider2D destination) {
        var dropping = carrying;
        this.carrying = null;

        // Center object on destination X when dropped
        dropping.transform.SetParent(null, true);
        dropping.transform
            .DOMoveX(destination.transform.position.x, 0.1f)
            .OnComplete(() => Util.MakeDynamic(dropping));

        PlayClip(dropOffClip);
    }

    void PlayClip(AudioClip clip) {
        audioSource.clip = clip;
        audioSource.Play();
    }

    void SetSprite(Sprite sprite) {
        spriteRenderer.sprite = sprite;
        shadowRenderer.sprite = sprite;
    }
}
