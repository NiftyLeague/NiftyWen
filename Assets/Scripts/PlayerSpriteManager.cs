using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerSpriteManager : MonoBehaviour
{
    public PlayerController playerController;
    [Space]
    public List<Sprite> importedCharacterSpriteSheets;
    public List<Sprite> demoCharacterSpriteSheets;
    public List<Sprite> allCharacterSpriteSheets;

    private int currentCharacterSpriteSheet;
    private bool hasBeenInitializedAlready;

    private void Start()
    {
        StartCoroutine(GetSpriteSheetsFromUrl());
    }

    private IEnumerator GetSpriteSheetsFromUrl()
    {
        UnityWebRequest uwr = UnityWebRequestTexture.GetTexture("https://nifty-league.s3.amazonaws.com/assets/sheets/92/1256.png");
        yield return uwr.SendWebRequest();

        if (uwr.isNetworkError)
        {
            Debug.Log("Error While Sending: " + uwr.error);
        }
        else
        {
            Debug.Log("GOT IT");
            Texture2D spriteSheetTexture = ((DownloadHandlerTexture)uwr.downloadHandler).texture;
            Sprite spriteSheetSprite = Sprite.Create(spriteSheetTexture, new Rect(0, 0, spriteSheetTexture.width, spriteSheetTexture.height), new Vector2(0.5f,0.5f), 16);
            importedCharacterSpriteSheets.Add(spriteSheetSprite);
        }

        InitializeSpriteSheets();
    }

    void InitializeSpriteSheets()
    {
        if (hasBeenInitializedAlready)
        {
            return;
        }

        if (importedCharacterSpriteSheets.Count > 0)
        {
            foreach (Sprite spriteSheet in importedCharacterSpriteSheets)
            {
                allCharacterSpriteSheets.Add(spriteSheet);
            }
        }

        foreach (Sprite spriteSheet in demoCharacterSpriteSheets)
        {
            allCharacterSpriteSheets.Add(spriteSheet);
        }

        hasBeenInitializedAlready = true;
    }

    public void ChangeCharacter()
    {
        currentCharacterSpriteSheet++;
        if (currentCharacterSpriteSheet > allCharacterSpriteSheets.Count - 1)
        {
            currentCharacterSpriteSheet = 0;
        }
        SetCharacterSpriteSheet();
    }

    void SetCharacterSpriteSheet()
    {
        playerController.spriteRenderer.sprite = allCharacterSpriteSheets[currentCharacterSpriteSheet];
        //playerController.playerSprites = characterSpriteSheet.ToArray();
    }
}
