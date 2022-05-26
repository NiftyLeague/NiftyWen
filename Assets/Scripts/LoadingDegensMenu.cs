using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoadingDegensMenu : MonoBehaviour
{
    public TextMeshProUGUI loadingDegensText;
    public List<string> loadingTexts;
    float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 3)
        {
            timer = 0;
        }
        loadingDegensText.text = loadingTexts[(int)timer];
    }
}
