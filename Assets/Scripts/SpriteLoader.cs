using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class SpriteLoader : MonoBehaviour
{
	public MenuManager menuManager;
	private static SpriteLoader I;

	public SpriteRenderer sr;
	public int id;

	[Space]
	public PlayerSpriteManager playerSpriteManager;
	public LoadingDegensMenu loadingDegensMenu;

	private void Awake()
	{
		I = this;
		loadingDegensMenu.gameObject.SetActive(true);
	}

	private void Start()
	{
		LoadSpritesheet($"https://d7ct17ettlkln.cloudfront.net/assets/sheets/92/{id}.png",
			16, 16, 128, 128, OnComplete);
	}

	private void OnComplete(List<Sprite> sprites)
	{
		Debug.Log(sprites[5]);
		loadingDegensMenu.CloseScreen();
		playerSpriteManager.GetSprites(sprites);
		menuManager.TurnOnMenu();

		//StartCoroutine(TestAnimation(sprites));
	}

    private IEnumerator TestAnimation(List<Sprite> sprites)
    {
        int frame = 0;
        int len = sprites.Count;
        while (true)
        {
            yield return new WaitForSeconds(0.07f);
            sr.sprite = sprites[frame++ % len];
        }
    }

    public static void LoadSpritesheet(string url, int columns, int rows, int cellWith, int cellHeight, Action<List<Sprite>> onComplete)
	{
		I.StartCoroutine(I._LoadSpritesheet(url, columns, rows, cellWith, cellHeight, onComplete));
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
		onComplete(null);
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
				if (frame % 4 == 0)
				{
					yield return new WaitForEndOfFrame();
				}
			}
		}
		onComplete(sprites);
	}
}
