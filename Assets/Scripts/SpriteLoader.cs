using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class SpriteLoader : MonoBehaviour
{
	//private static SpriteLoader I;

	//public SpriteRenderer sr;
	//public int id;

	//[Space]
	public PlayerSpriteManager playerSpriteManager;

	static int degensToLoadMin;
	static int degensToLoadMax;
	static List<int> degenIDsToLoad;

	//private void Awake()
	//{
	//	I = this;
	//}

	private void Start()
	{
		List<int> degenIDsToLoad = new List<int>();
		degenIDsToLoad.Add(2);
		degenIDsToLoad.Add(3);
		degenIDsToLoad.Add(1256);
		GetDegensFromIDs(degenIDsToLoad);
	}

	private void GetDegensFromIDs(List<int> degenIDs)
	{
		degenIDsToLoad = new List<int>();
		degenIDsToLoad = degenIDs;
		degensToLoadMax = degenIDs.Count;
		if (degensToLoadMax > 0)
		{
			degensToLoadMin = 1;
		}
		playerSpriteManager.LoadingDegensText(degensToLoadMin, degensToLoadMax);

		GetDegen(degenIDsToLoad[0]);
	}

	private void GetDegen(int degenID)
	{
		LoadSpritesheet($"https://d7ct17ettlkln.cloudfront.net/assets/sheets/92/{degenID}.png",
		16, 16, 128, 128, OnComplete);
	}

	private void OnComplete(List<Sprite> sprites)
	{
		//Debug.Log(sprites[5]);		
		playerSpriteManager.GetSprites(sprites);
		Debug.Log("Degen Complete!");
		degensToLoadMin++;
		playerSpriteManager.LoadingDegensText(degensToLoadMin, degensToLoadMax);
		if (degensToLoadMin <= degensToLoadMax)
		{
			GetDegen(degenIDsToLoad[degensToLoadMin - 1]);
		}

		//StartCoroutine(TestAnimation(sprites));
	}

    //private IEnumerator TestAnimation(List<Sprite> sprites)
    //{
    //    int frame = 0;
    //    int len = sprites.Count;
    //    while (true)
    //    {
    //        yield return new WaitForSeconds(0.07f);
    //        sr.sprite = sprites[frame++ % len];
    //    }
    //}

    public void LoadSpritesheet(string url, int columns, int rows, int cellWith, int cellHeight, Action<List<Sprite>> onComplete)
	{
		//I.StartCoroutine(I._LoadSpritesheet(url, columns, rows, cellWith, cellHeight, onComplete));
		StartCoroutine(_LoadSpritesheet(url, columns, rows, cellWith, cellHeight, onComplete));
	}

	private IEnumerator _LoadSpritesheet(string url, int columns, int rows, int cellWith, int cellHeight, Action<List<Sprite>> onComplete)
	{
		//yield return new WaitForSecondsRealtime(4f);
		UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
		yield return www.SendWebRequest();

		if (www.result != UnityWebRequest.Result.Success)
		{
			Debug.Log(www.error);
		}
		else
		{
			Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
			texture.filterMode = FilterMode.Point;
			yield return GetSprites(texture, columns, rows, cellWith, cellHeight, onComplete);
		}
		//onComplete(null);
	}

	private IEnumerator GetSprites(Texture2D texture, int columns, int rows, int cellWith, int cellHeight, Action<List<Sprite>> onComplete)
	{
		if (texture.width != columns * cellWith || texture.width != rows * cellHeight)
		{
			Debug.Log($"The dimension of the texture ({texture.width}x{texture.height}) does not match the column and rows and cell dimension provided");
			yield break;
		}

		int frame = 0;
		int height = rows * cellHeight;
		List<Sprite> sprites = new List<Sprite>();
		for (int j = 1; j <= rows; j++)
		{
			for (int i = 0; i < columns; i++)
			{
				Sprite s = Sprite.Create(texture, new Rect(i * cellWith, height - (j * cellHeight), cellWith, cellHeight), new Vector2(0.5f, 0.5f), 16);
				s.name = frame.ToString();
				sprites.Add(s);
				frame++;
				//if (frame % 4 == 0)
				if (frame % 2 == 0)
				{
					yield return new WaitForEndOfFrame();
				}
			}
		}
		onComplete(sprites);
	}
}
