using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        InitializeSpriteSheets();
    }

    void InitializeSpriteSheets()
    {
        if (hasBeenInitializedAlready)
        {
            return;
        }

        //foreach (Sprite spriteSheet in importedCharacterSpriteSheets)
        //{
        //    allCharacterSpriteSheets.Add(spriteSheet);
        //}

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
