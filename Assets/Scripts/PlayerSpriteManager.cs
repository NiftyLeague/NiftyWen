using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerSpriteManager : MonoBehaviour
{
    CharacterAnimator characterAnimator;

    private static List<CharacterSprites> importedCharacterSprites;
    public List<CharacterSprites> demoCharacterSprites;

    public static bool canChangeCharacters;
    private int currentCharacterSprites;
    private static bool hasBeenInitializedAlready;

    private static bool created;

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
        Scene scene = SceneManager.GetActiveScene();
        if (scene.buildIndex == 1)
        {
            ChangeCharacter();
        }
    }

    public void GetSprites(List<Sprite> sprites)
    {
        Debug.Log(sprites[5]);
        InitializeSprites();
        CharacterSprites newImportedCharacterSprites = new CharacterSprites();
        newImportedCharacterSprites.sprites = new List<Sprite>();
        foreach (Sprite sprite in sprites)
        {
            newImportedCharacterSprites.sprites.Add(sprite);
        }
        importedCharacterSprites.Add(newImportedCharacterSprites);
        canChangeCharacters = true;
    }

    void InitializeSprites()
    {
        if (hasBeenInitializedAlready)
        {
            return;
        }

        importedCharacterSprites = new List<CharacterSprites>();

        hasBeenInitializedAlready = true;
    }

    public void ChangeCharacter()
    {
        if (canChangeCharacters)
        {
            currentCharacterSprites++;
            if (currentCharacterSprites > importedCharacterSprites.Count - 1)
            {
                currentCharacterSprites = 0;
            }
        }
        else
        {
            currentCharacterSprites = UnityEngine.Random.Range(0, 6);
        }

        SetCharacterSprites();
    }

    public bool CanChangeCharacters()
    {
        if (importedCharacterSprites.Count <= 1)
        {
            canChangeCharacters = false;
        }

        return canChangeCharacters;
    }

    public void SetCharacterSprites()
    {
        if (characterAnimator == null)
        {
            characterAnimator = FindObjectOfType<CharacterAnimator>();
        }

        List<Sprite> spritesToUse = new List<Sprite>();

        if (canChangeCharacters)
        {
            foreach (Sprite sprite in importedCharacterSprites[currentCharacterSprites].sprites)
            {
                spritesToUse.Add(sprite);
            }
        }
        else
        {
            foreach (Sprite sprite in demoCharacterSprites[currentCharacterSprites].sprites)
            {
                spritesToUse.Add(sprite);
            }
        }

        characterAnimator.idle.Clear();
        characterAnimator.idle.Add(spritesToUse[0]);
        characterAnimator.idle.Add(spritesToUse[0]);
        characterAnimator.idle.Add(spritesToUse[0]);

        characterAnimator.run.Clear();
        for (int i = 9; i <= 16; i++)
            characterAnimator.run.Add(spritesToUse[i]);
        
        characterAnimator.jumpLaunch.Clear();
        characterAnimator.jumpLaunch.Add(spritesToUse[59]);
        characterAnimator.jumpLaunch.Add(spritesToUse[59]);

        characterAnimator.jumpUp.Clear();
        for (int i = 59; i <= 60; i++)
            characterAnimator.jumpUp.Add(spritesToUse[i]);

        characterAnimator.jumpDown.Clear();
        for (int i = 64; i <= 65; i++)
            characterAnimator.jumpDown.Add(spritesToUse[i]);

        characterAnimator.skidLand = spritesToUse[88];
        characterAnimator.skid.Clear();
        for (int i = 89; i <= 90; i++)
            characterAnimator.skid.Add(spritesToUse[i]);
        characterAnimator.skidRecover = spritesToUse[91];

        characterAnimator.somersault.Clear();
        for (int i = 66; i <= 73; i++)
            characterAnimator.somersault.Add(spritesToUse[i]);

        characterAnimator.attackCharge.Clear();
        for (int i = 19; i <= 24; i++)
            characterAnimator.attackCharge.Add(spritesToUse[i]);

        characterAnimator.attack.Clear();
        for (int i = 25; i <= 28; i++)
            characterAnimator.attack.Add(spritesToUse[i]);

        characterAnimator.attackRecover.Clear();
        for (int i = 27; i <= 28; i++)
            characterAnimator.attackRecover.Add(spritesToUse[i]);

        //characterAnimator.impact.Clear();
        //for (int i = 80; i <= 81; i++)
        //    characterAnimator.impact.Add(spritesToUse[i]);

        characterAnimator.wallSlide.Clear();
        for (int i = 75; i <= 76; i++)
            characterAnimator.wallSlide.Add(spritesToUse[i]);

        characterAnimator.wallSlideJumpLaunch.Clear();
        characterAnimator.wallSlideJumpLaunch.Add(spritesToUse[59]);
        characterAnimator.wallSlideJumpLaunch.Add(spritesToUse[59]);
    }
}

[Serializable]
public class CharacterSprites
{
    public List<Sprite> sprites;
}
