using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameplayManager : MonoBehaviour
{
    public int score;
    public float currentTimeScale = 1.0f;
    [Space]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI wenMessageText;
    public TextMeshProUGUI soonMessageText;
    public TextMeshProUGUI instructionMessageText;
    [Space]
    public SimpleAnim ballMachineAnim;
    public Sprite[] ballMachineSpritesIdle;
    public Sprite[] ballMachineSpritesFire;
    [Space]
    public ProjectileHitter projectileHitter;
    public GameObject ballPrefab;
    public Transform ballStartLocation;

    string[] wenMessages;
    float nextBallTimer = 3;

    private InputState input = new InputState();

    void Awake()
    {
        UpdateScoreText();
        ResetWenMessageTexts();
        InitializeWenMessages();
        StartCoroutine(InstructionMessageFade());
    }

    void Update()
    {
        nextBallTimer += Time.deltaTime;

        if (nextBallTimer >= 5)
        {
            StartCoroutine(PlayWenMessages());

            nextBallTimer = 0;
        }

        InputReader.GetInput(input);

        if (input.PressedA)
        {
            projectileHitter.TurnOn();
        }
    }

    public void ScorePoint()
    {
        score++;
        currentTimeScale += 0.04f;
        Time.timeScale = currentTimeScale;
        UpdateScoreText();
    }

    void UpdateScoreText()
    {
        scoreText.text = score.ToString("0");
    }

    public void Lose()
    {
        score = 0;
        currentTimeScale = 0;
        Time.timeScale = currentTimeScale;
        UpdateScoreText();
    }

    void InitializeWenMessages()
    {
        TextAsset wenMessagesFile = Resources.Load<TextAsset>("wen");

        wenMessages = wenMessagesFile.text.Split(new char[] { '\n' });
    }

    void ResetWenMessageTexts()
    {
        wenMessageText.transform.localScale = new Vector3(0, 0, 0);
        soonMessageText.transform.localScale = new Vector3(0, 0, 0);
    }

    string GetRandomWenMessage()
    {
        return wenMessages[Random.Range(0, wenMessages.Length-1)].ToString();
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

    IEnumerator PlayWenMessages()
    {
        ResetWenMessageTexts();
        wenMessageText.text = GetRandomWenMessage();

        float a = 0;
        float b = 0.1f;

        Tween<float> scaleTween = new Tween<float>(a, b, 1f, TweenEaseType.CubicIn);

        while (!scaleTween.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            wenMessageText.transform.localScale = new Vector3(scaleTween.Update(Time.deltaTime), scaleTween.Update(Time.deltaTime), scaleTween.Update(Time.deltaTime));
        }

        yield return new WaitForSeconds(1);
        ResetWenMessageTexts();

        a = 0;
        b = 0.1f;

        scaleTween = new Tween<float>(a, b, 1f, TweenEaseType.CubicIn);

        while (!scaleTween.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            soonMessageText.transform.localScale = new Vector3(scaleTween.Update(Time.deltaTime), scaleTween.Update(Time.deltaTime), scaleTween.Update(Time.deltaTime));
        }

        yield return new WaitForSeconds(1);

        ResetWenMessageTexts();

        ballMachineAnim.Play(ballMachineSpritesFire, false);

        yield return new WaitForSeconds(0.2f);

        SpawnProjectile();

        yield return new WaitForSeconds(0.3f);

        ballMachineAnim.Play(ballMachineSpritesIdle, false);
    }

    void SpawnProjectile()
    {
        var newProjectile = Instantiate(ballPrefab, ballStartLocation);
        newProjectile.transform.localPosition = new Vector3(0, 0, 0);
    }
}
