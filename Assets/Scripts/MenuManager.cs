using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public MainMenuManager mainMenuManager;
    public GameplayManager gameplayManager;
    public AudioManager audioManager;
    [Space]
    public List<MenuOption> menuOptions;
    public RectTransform menuCursor;
    public GameObject menuPanel;
    public GameObject whiteFlash;
    [Space]
    public Color32 optionColorDefault;
    public Color32 optionColorPressed;

    private int selectedMenuOption;
    private bool canSelectMenuOptions;
    private bool isInSecondaryMainMenu;
    private InputState input = new InputState();

    void Start()
    {
        ResetMenuOptions();
        SetSelectedMenuOption();
    }

    void Update()
    {
        InputReader.GetInput(input);

        if (isInSecondaryMainMenu && input.PressedB)
        {
            mainMenuManager.GoBack();
            TurnOnMenu();
            ResetMenuOptions();
            return;
        }

        if (!canSelectMenuOptions)
        {
            return;
        }

        if (input.PressedUp)
        {
            ChangeMenuOption(-1);
        }

        if (input.PressedDown)
        {
            ChangeMenuOption(1);
        }

        if (input.PressedA || input.PressedB || input.PressedX || input.PressedY || input.PressedStart)
        {
            StartCoroutine(SelectOption());
        }
    }

    public void TurnOnMenu()
    {
        canSelectMenuOptions = true;
        menuPanel.SetActive(true);
    }

    void ChangeMenuOption(int menuOptionChange)
    {
        audioManager.PlaySound(AudioManager.SoundID.messagePopup);

        selectedMenuOption = selectedMenuOption + menuOptionChange;

        if (selectedMenuOption > menuOptions.Count - 1)
        {
            selectedMenuOption = 0;
        }

        if (selectedMenuOption < 0)
        {
            selectedMenuOption = menuOptions.Count - 1;
        }

        SetSelectedMenuOption();
    }

    void SetSelectedMenuOption()
    {
        menuCursor.anchoredPosition = new Vector2(menuCursor.anchoredPosition.x, menuOptions[selectedMenuOption].menuOptionText.rectTransform.anchoredPosition.y);

        //Debug.Log("Currently Selected Option: " + menuOptions[selectedMenuOption].menuOptionString);
    }

    void ResetMenuOptions()
    {
        foreach (MenuOption option in menuOptions)
        {
            option.menuOptionText.text = option.menuOptionString;
        }
    }

    IEnumerator SelectOption()
    {
        canSelectMenuOptions = false;

        audioManager.PlaySound(AudioManager.SoundID.menuOptionSelect);

        foreach (MenuOption option in menuOptions)
        {
            option.menuOptionText.text = "";
        }

        menuOptions[selectedMenuOption].menuOptionText.text = menuOptions[selectedMenuOption].menuOptionString;

        whiteFlash.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.05f);

        whiteFlash.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.1f);

        menuOptions[selectedMenuOption].menuOptionText.color = optionColorPressed;

        yield return new WaitForSeconds(0.05f);

        menuOptions[selectedMenuOption].menuOptionText.color = optionColorDefault;

        yield return new WaitForSeconds(0.05f);

        menuOptions[selectedMenuOption].menuOptionText.color = optionColorPressed;

        yield return new WaitForSeconds(0.05f);

        menuOptions[selectedMenuOption].menuOptionText.color = optionColorDefault;

        yield return new WaitForSeconds(1f);

        switch (menuOptions[selectedMenuOption].menuOptionType)
        {
            case MenuType.MainMenuPlay:
                SceneManager.LoadScene(1);
                break;
            case MenuType.MainMenuHowToPlay:
                mainMenuManager.GoToHowToPlayScreen();
                menuPanel.SetActive(false);
                isInSecondaryMainMenu = true;
                ResetMenuOptions();
                break;
            case MenuType.MainMenuLeaderboard:
                mainMenuManager.GoToLeaderboardsScreen();
                menuPanel.SetActive(false);
                isInSecondaryMainMenu = true;
                ResetMenuOptions();
                break;
            case MenuType.MainMenuAbout:
                mainMenuManager.GoToAboutScreen();
                menuPanel.SetActive(false);
                isInSecondaryMainMenu = true;
                ResetMenuOptions();
                break;
            case MenuType.MainMenuQuit:
                Application.Quit();
                break;
            case MenuType.GameplayTryAgain:
                menuPanel.SetActive(false);
                ResetMenuOptions();
                gameplayManager.ResetEverythingForANewGame();
                break;
            case MenuType.GameplayQuit:
                SceneManager.LoadScene(0);
                break;
        }
    }
}

[Serializable]
public class MenuOption
{
    public string menuOptionString;
    public TextMeshProUGUI menuOptionText;
    public MenuType menuOptionType;
}

public enum MenuType
{
    None,
    MainMenuPlay,
    MainMenuHowToPlay,
    MainMenuLeaderboard,
    MainMenuAbout,
    MainMenuQuit,
    GameplayTryAgain,
    GameplayQuit,
}
