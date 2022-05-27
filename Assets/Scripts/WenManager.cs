using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WenManager : MonoBehaviour
{
    private static bool created;
    private static string[] wenMessages;

    void Awake()
    {
        if (created == false)
        {
            created = true;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    private void Start()
    {
        StartCoroutine(InitializeWenMessages());
    }

    IEnumerator InitializeWenMessages()
    {
        UnityWebRequest www = UnityWebRequest.Get("https://nifty-league.s3.amazonaws.com/assets/wen/wen.csv");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
            TextAsset wenMessagesFile = Resources.Load<TextAsset>("wen");
            wenMessages = wenMessagesFile.text.Split(new char[] { '\n' });
        }
        else
        {
            wenMessages = www.downloadHandler.text.Split(new char[] { '\n' });
            Debug.Log(www.downloadHandler.text);
        }
    }

    public string GetRandomWenMessage()
    {
        return wenMessages[Random.Range(0, wenMessages.Length - 1)].ToString();
    }
}
