using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manage the game state, score, lives and enemies.
/// </summary>
public class GameController : MonoBehaviour {

    public GameObject enemyPrefab;
    public GameObject dieEffectPrefab;
    public AudioClip startGameClip;
    public AudioClip endGameClip;
    public AudioClip dieClip;
    public Player player;
    public Rocket rocket;

    AudioSource audioSource;

    Button startButton;
    Text scoreText;
    Text livesText;
    Text highScoreText;
    int lives;
    int score;
    int level;

    void Start() {
        audioSource = GetComponent<AudioSource>();

        startButton = GameObject.Find("StartButton").GetComponent<Button>();
        scoreText = GameObject.Find("ScoreText").GetComponent<Text>();
        livesText = GameObject.Find("LivesText").GetComponent<Text>();
        highScoreText = GameObject.Find("HighScoreText").GetComponent<Text>();

        player.gameObject.SetActive(false);

        UpdateUI();

        ShowStartButton();
    }

    void Update() {
    }

    void CreateEnemy() {
        var x = (Random.value > 0.5f) ? -22 : +22;
        var y = Random.Range(-10f, +11f);
        Instantiate(enemyPrefab, new Vector3(x, y, 0), Quaternion.identity, transform);
    }

    void DestroyEnemy(GameObject enemy) {
        enemy.transform.parent = null;
        var sprite = enemy.GetComponent<SpriteRenderer>();
        sprite.DOFade(0, 0.3f)
            .OnComplete(() => GameObject.Destroy(enemy));
    }

    int NumEnemies { get { return transform.childCount; } }

    GameObject GetEnemy(int i) {
        return transform.GetChild(0).gameObject;
    }

    void CreateAllEnemies() {
        var numEnemiesExpected = level + 3;
        while (NumEnemies < numEnemiesExpected) {
            CreateEnemy();
        }
    }
    public void RemoveAllEnemies() {
        while (NumEnemies > 0) {
            DestroyEnemy(GetEnemy(0));
        }
    }

    void StartLevel() {
        RemoveAllEnemies();

        player.gameObject.SetActive(true);
        player.transform.position = new Vector3(4, -9, 0);

        CreateAllEnemies();
    }

    public void StartGame() {
        HideStartButton();

        level = 0;
        lives = 4;
        score = 0;
        UpdateUI();

        rocket.StartGame();
        PlayClip(startGameClip);

        StartLevel();
    }

    public void StartNextLevel() {
        level++;

        StartLevel();
    }

    public void EndGame() {
        RemoveAllEnemies();
        PlayClip(endGameClip);

        player.gameObject.SetActive(false);
        rocket.EndGame();

        ShowStartButton();

        UpdateUI();
    }

    public void Score(int points) {
        score += points;

        UpdateUI();
    }

    public void PlayerDies() {

        lives -= 1;
        UpdateUI();

        RunDieEffect(player.gameObject, "StartNextLife");

        // Prevent future events happening to the player (like dying twice or collecting fuel)
        player.gameObject.SetActive(false);
    }

    void StartNextLife() {
        if (lives == 0) {
            EndGame();
        }
        else {
            StartLevel();
        }
    }

    public void EnemyDies(GameObject enemy) {
        RunDieEffect(enemy, "CreateEnemy");
        DestroyEnemy(enemy);
    }

    void RunDieEffect(GameObject gameObject, string completeMethod) {
        var dieEffect = GameObject.Instantiate(dieEffectPrefab, gameObject.transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
        Destroy(dieEffect.gameObject, dieEffect.main.duration);
        Invoke(completeMethod, dieEffect.main.duration);
        PlayClip(dieClip);
    }

    void PlayClip(AudioClip clip) {
        audioSource.Stop();
        if (clip != null) {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    void UpdateUI() {
        livesText.text = lives.ToString();
        scoreText.text = score.ToString();

        var highScore = PlayerPrefs.GetInt("High Score", 0);
        if (score > highScore) {
            highScore = score;
            PlayerPrefs.SetInt("High Score", score);
        }
        highScoreText.text = highScore.ToString();
    }

    void ShowStartButton() {
        startButton.enabled = true;
        startButton.transform.DOMoveY(0, 0.5f).SetEase(Ease.InOutBounce);
    }

    void HideStartButton() {
        startButton.transform.DOMoveY(500, 0.2f).SetEase(Ease.InOutQuint);
        startButton.enabled = false;
    }
}
