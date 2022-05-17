using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameplayManager : MonoBehaviour
{
    public TextMeshProUGUI wenMessageText;
    public TextMeshProUGUI soonMessageText;

    string[] wenMessages;
    float nextBallTimer;

    void Awake()
    {
        ResetWenMessageTexts();
        InitializeWenMessages();
    }

    void Update()
    {
        nextBallTimer += Time.deltaTime;

        if (nextBallTimer >= 5)
        {
            StartCoroutine(PlayWenMessages());

            nextBallTimer = 0;
        }   
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
    }
}
