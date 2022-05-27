using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameplayManager : MonoBehaviour
{
    public MenuManager menuManager;
    public AudioManager audioManager;
    public WenManager wenManager;
    public Character playerCharacter;
    [Space]
    public int score;
    public int highScore;
    public float timePlayed;
    public int ballsTotal;
    public int ballsDodged;
    public int bombsDodged;
    [Space]
    public float currentSpeedIncrease;
    public CameraShake cameraShake;
    [Space]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI wenMessageText;
    public TextMeshProUGUI soonMessageText;
    public TextMeshProUGUI instructionMessageText;
    public TextMeshProUGUI gameOverStatNamesText;
    public TextMeshProUGUI gameOverStatNumbersText;
    public TextMeshProUGUI gameOverLeaderboardPositionsText;
    public TextMeshProUGUI gameOverLeaderboardNamesText;
    public TextMeshProUGUI gameOverLeaderboardScoresText;
    [Space]
    public Transform playerTransform;
    public Transform doublePointLineTransform;
    [Space]
    public SimpleAnim ballMachineAnim;
    public Sprite[] ballMachineSpritesIdle;
    public Sprite[] ballMachineSpritesFire;
    [Space]
    public GameObject ballPrefab;
    public GameObject bombPrefab;
    public Transform ballStartLocation;

    float nextBallTimer;
    [HideInInspector] public bool nextBallReadyToLaunch;
    bool hasLaunchedABomb;
    public bool hasGameEnded;
    int bombsFiredInARow;

    private InputState input = new InputState();

    void Awake()
    {
        UpdateScoreText();
        ResetEverythingForANewGame();
        ResetWenMessageTexts();
        menuManager.menuPanel.SetActive(false);
        StartCoroutine(InstructionMessageFade());
    }

    void Update()
    {
        if (hasGameEnded)
        {
            return;
        }

        nextBallTimer += Time.deltaTime;

        if (nextBallTimer >= 4 && nextBallReadyToLaunch)
        {
            StartCoroutine(PlayWenMessages());
            nextBallReadyToLaunch = false;
            nextBallTimer = 0;
        }
    }

    private void FixedUpdate()
    {
        if (hasGameEnded)
        {
            return;
        }

        timePlayed += Time.deltaTime;
    }

    public void ScorePoint()
    {
        nextBallReadyToLaunch = true;
        cameraShake.Shake(0.2f, 10);
        if (playerTransform.position.x >= doublePointLineTransform.position.x)
        {
            score += 2;
        }
        else
        {
            score++;
        }
        StartCoroutine(AnimateScoreText());
        IncreaseSpeed();
        UpdateScoreText();
    }

    void UpdateScoreText()
    {
        scoreText.text = score.ToString("0");
    }

    public void Lose()
    {
        hasGameEnded = true;
        cameraShake.Shake(0.5f, 5);
        if (score > highScore)
        {
            highScore = score;
        }
        IncreaseSpeed(true);
        playerCharacter.Lose();
        StartCoroutine(PlayGameOverScreen());
    }

    public void Dodge(bool isABomb)
    {
        if (isABomb)
        {
            bombsDodged++;
        }
        else
        {
            ballsDodged++;
        }
        nextBallReadyToLaunch = true;
        IncreaseSpeed();
    }

    public void IncreaseSpeed(bool reset = false)
    {
        currentSpeedIncrease += 0.5f;
        if (reset)
        {
            currentSpeedIncrease = 0;
        }
    }

    public void ResetEverythingForANewGame()
    {
        score = 0;
        ballsTotal = 0;
        ballsDodged = 0;
        timePlayed = 0;
        bombsDodged = 0;

        gameOverStatNamesText.text = "";
        gameOverStatNumbersText.text = "";
        gameOverLeaderboardNamesText.gameObject.SetActive(false);
        gameOverLeaderboardPositionsText.gameObject.SetActive(false);
        gameOverLeaderboardScoresText.gameObject.SetActive(false);

        bombsFiredInARow = 0;
        hasGameEnded = false;
        hasLaunchedABomb = false;
        nextBallReadyToLaunch = true;

        playerCharacter.UnLose();

        UpdateScoreText();
    }

    void ResetWenMessageTexts()
    {
        wenMessageText.transform.localScale = new Vector3(0, 0, 0);
        soonMessageText.transform.localScale = new Vector3(0, 0, 0);
    }

    IEnumerator InstructionMessageFade()
    {
        yield return new WaitForSeconds(8);

        int a = 255;
        int b = 0;

        Tween<float> alphaTween = new Tween<float>(a, b, 0.5f, TweenEaseType.CubicInOut);

        while (!alphaTween.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            byte alphaTweenByte = (byte)alphaTween.Update(Time.deltaTime);
            instructionMessageText.color = new Color32(0, 153, 219, alphaTweenByte);
        }
    }

    IEnumerator PlayGameOverScreen()
    {
        audioManager.PlaySound(AudioManager.SoundID.lose);

        scoreText.text = "GAME OVER";

        yield return new WaitForSeconds(3);

        scoreText.text = "";

        gameOverStatNamesText.text = "SCORE\nHIGH SCORE\nTIME PLAYED\nTOTAL BALLS\nBALLS DODGED";

        float secondsPlayed = timePlayed % 60;
        float minutesPlayed = timePlayed / 60;
        float hoursPlayed = timePlayed / 60 / 60;
        gameOverStatNumbersText.text = score.ToString("0") + "\n" + highScore.ToString("0") + "\n" + hoursPlayed.ToString("0") + ":" + minutesPlayed.ToString("00") + ":" + secondsPlayed.ToString("00") + "\n" + ballsTotal.ToString("0") + "\n" + ballsDodged.ToString("0");

        if (hasLaunchedABomb)
        {
            gameOverStatNamesText.text += "\nBOMBS DODGED";
            gameOverStatNumbersText.text += "\n" + bombsDodged.ToString("0");
        }

        yield return new WaitForSeconds(5);

        gameOverStatNamesText.text = "";
        gameOverStatNumbersText.text = "";

        gameOverLeaderboardNamesText.gameObject.SetActive(true);
        gameOverLeaderboardPositionsText.gameObject.SetActive(true);
        gameOverLeaderboardScoresText.gameObject.SetActive(true);

        menuManager.TurnOnMenu();
    }

    IEnumerator PlayWenMessages()
    {
        ResetWenMessageTexts();
        audioManager.PlaySound(AudioManager.SoundID.messagePopup);
        wenMessageText.text = wenManager.GetRandomWenMessage();

        float a = 0;
        float b = 0.1f;

        Tween<float> scaleTween = new Tween<float>(a, b, 0.2f, TweenEaseType.CubicIn);

        while (!scaleTween.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            wenMessageText.transform.localScale = new Vector3(0.1f, scaleTween.Update(Time.deltaTime), 0.1f);
        }

        yield return new WaitForSeconds(1.5f);
        ResetWenMessageTexts();
        audioManager.PlaySound(AudioManager.SoundID.messagePopup);

        a = 0;
        b = 0.1f;

        scaleTween = new Tween<float>(a, b, 0.2f, TweenEaseType.CubicIn);

        while (!scaleTween.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            soonMessageText.transform.localScale = new Vector3(0.1f, scaleTween.Update(Time.deltaTime), 0.1f);
        }

        float randomTime = Random.Range(0.5f, 2.0f);

        yield return new WaitForSeconds(randomTime);

        ResetWenMessageTexts();

        ballMachineAnim.Play(ballMachineSpritesFire, false);

        yield return new WaitForSeconds(0.2f);

        SpawnProjectile();

        yield return new WaitForSeconds(0.3f);

        ballMachineAnim.Play(ballMachineSpritesIdle, false);
    }

    IEnumerator AnimateScoreText()
    {
        float a = 0.09f;
        float b = 0.1f;

        Tween<float> scaleTween = new Tween<float>(a, b, 0.5f, TweenEaseType.CubicIn);

        while (!scaleTween.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            scoreText.transform.localScale = new Vector3(scaleTween.Update(Time.deltaTime), scaleTween.Update(Time.deltaTime), scaleTween.Update(Time.deltaTime));
        }

        audioManager.PlaySound(AudioManager.SoundID.gainPoint);
    }

    void SpawnProjectile()
    {
        GameObject chosenProjectile = ballPrefab;
        if (score >= 10)
        {
            if (Random.Range(0, 100 + (int)(score / 2)) >= 50)
            {
                chosenProjectile = bombPrefab;
                hasLaunchedABomb = true;
                bombsFiredInARow++;

                if (bombsFiredInARow >= 10)
                {
                    chosenProjectile = ballPrefab;
                }
            }
            else
            {
                bombsFiredInARow = 0;
            }
        }

        var newProjectile = Instantiate(chosenProjectile, ballStartLocation);
        newProjectile.transform.localPosition = new Vector3(0, 0, 0);
        var spawnedProjectile = newProjectile.GetComponent<Projectile>();
        spawnedProjectile.SetNewSpeed(currentSpeedIncrease);
        if (!spawnedProjectile.isABomb)
        {
            ballsTotal++;
        }

        audioManager.PlaySound(AudioManager.SoundID.projectileShoot);
    }
}
