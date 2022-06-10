using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using UnityEngine.SceneManagement;
using CodeStage.AntiCheat.ObscuredTypes;

public class MenuManager : Singleton<MenuManager>
{
	public MainMenuManager mainMenuManager;
	public GameplayManager gameplayManager;
	public AudioManager audioManager;
	[Space]
	public List<MenuOption> menuOptions;
	public RectTransform menuCursor;
	public GameObject menuPanel;
	public GameObject tokensPanel;
	public GameObject purchaseTokensPanel;
	public GameObject whiteFlash;
	public float menuCursorYOffset;
	[Space]
	public TextMeshProUGUI tokenAmountText;
	public Color32 tokenAmountDefaultColor;
	public TextMeshProUGUI tokenCurrentPriceText;
	public TextMeshProUGUI nftlCurrentOwnedText;
	public RectTransform tokensPanelRect;
	public float tokenPurchaseAnimSpeed;
	public TweenEaseType tokenPurchaseAnimTweenEaseType;
	private Coroutine currentTokenPurchaseAnimationCoroutine;
	private Vector2 tokenPanelStartPosition;
	[Space]
	public Color32 optionColorDefault;
	public Color32 optionColorPressed;
	[Space]
	public TextMeshProUGUI leaderboardTitleText;
	public TextMeshProUGUI leaderboardPositionsText;
	public TextMeshProUGUI leaderboardNamesText;
	public TextMeshProUGUI leaderboardScoresText;
	[Space]
	public TextMeshProUGUI errorMessageText;
	private Coroutine currentErrorMessageCoroutine;

	[HideInInspector] public ObscuredInt leaderboardToShow;

	List<ObscuredString> leaderboardAllTimeNames;
	List<ObscuredInt> leaderboardAllTimeScores;

	List<ObscuredString> leaderboardMonthlyNames;
	List<ObscuredInt> leaderboardMonthlyScores;

	List<ObscuredString> leaderboardWeeklyNames;
	List<ObscuredInt> leaderboardWeeklyScores;

	private MenuType lastSelectedMenuOption;
	private int selectedMenuOption;
	private bool canSelectMenuOptions;
	private bool isInSecondaryMainMenu;
	private float nftlOwned;
	private float currentTokenPrice;
	private uint tokensOwned;
	private InputState input = new InputState();

	void Start()
	{
		//TESTING, DELETE LATER
		nftlOwned = 56.54f;
		currentTokenPrice = 12.23f;
		//tokensOwned = 50;

		UpdateTokenAmount();
		ResetMenuOptions();
		SetSelectedMenuOption();
		UpdateLeaderboards();
		
		tokenPanelStartPosition = tokensPanelRect.anchoredPosition;
	}

	void Update()
	{
		InputReader.GetInput(input);

		if (isInSecondaryMainMenu)
		{
			if (input.PressedB)
			{
				audioManager.PlaySound(AudioManager.SoundID.batSwing);
				if (mainMenuManager != null)
				{
					mainMenuManager.GoBack();
				}
				purchaseTokensPanel.SetActive(false);
				UpdateLeaderboardDisplay();
				SetMenuEnabled(true);
				ResetMenuOptions();
				return;
			}

			if (input.PressedA)
			{
				if (lastSelectedMenuOption == MenuType.MainMenuLeaderboard)
				{
					ChangeCurrentLeaderboard();
				}

				if (lastSelectedMenuOption == MenuType.GameplayTryAgain || lastSelectedMenuOption == MenuType.MainMenuPlay)
				{
					PurchaseToken();
				}
				return;
			}

		}

		if (!canSelectMenuOptions || !menuPanel.gameObject.activeSelf)
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

		if (input.PressedA || input.PressedStart)
		{
			StartCoroutine(SelectOption());
		}
	}

	public void SetMenuEnabled(bool enabled)
	{
		tokensPanel.SetActive(enabled);
		isInSecondaryMainMenu = !enabled;
		canSelectMenuOptions = enabled;
		menuPanel.SetActive(enabled);
		ResetMenuOptions();
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
		menuCursor.anchoredPosition = new Vector2(menuCursor.anchoredPosition.x, menuOptions[selectedMenuOption].menuOptionText.rectTransform.anchoredPosition.y + menuCursorYOffset);
	}

	void ResetMenuOptions()
	{
		foreach (MenuOption option in menuOptions)
		{
			option.menuOptionText.text = option.menuOptionString;
		}

		if (tokensOwned <= 0)
		{
			menuOptions[0].menuOptionText.text = "PURCHASE TOKENS";
		}

		errorMessageText.text = "";
	}

	public void GainToken()
	{
		tokensOwned += 1;
		UpdateTokenAmount();
		audioManager.PlaySound(AudioManager.SoundID.gainPoint);
	}

	public void SpendToken()
	{
		tokensOwned -= 1;
		UpdateTokenAmount();
	}

	public void SetTokenBalance(uint balance)
	{
		tokensOwned = balance;
		UpdateTokenAmount();
	}

	public void UpdateTokenAmount()
	{
		tokenAmountText.color = tokenAmountDefaultColor;

		if (tokensOwned <= 0)
		{
			tokenAmountText.color = Color.red;
		}

		tokenAmountText.text = tokensOwned.ToString("000");

		ResetMenuOptions();
	}

	IEnumerator SelectOption()
	{
		canSelectMenuOptions = false;
		var menu = menuOptions[selectedMenuOption];
		lastSelectedMenuOption = menu.menuOptionType;
		audioManager.PlaySound(AudioManager.SoundID.menuOptionSelect);

		foreach (MenuOption option in menuOptions)
		{
			option.menuOptionText.text = "";
		}

		menu.menuOptionText.text = menu.menuOptionString;

		if (menu.menuOptionType == MenuType.MainMenuPlay || menu.menuOptionType == MenuType.GameplayTryAgain)
		{
			if (tokensOwned > 0)
			{
				audioManager.PlaySound(AudioManager.SoundID.insertCoin);
				SpendToken();
			}
			else
			{
				menu.menuOptionText.text = "PURCHASE TOKENS";
			}
		}

		whiteFlash.gameObject.SetActive(true);

		yield return new WaitForSeconds(0.05f);

		whiteFlash.gameObject.SetActive(false);

		yield return new WaitForSeconds(0.1f);

		menu.menuOptionText.color = optionColorPressed;

		yield return new WaitForSeconds(0.05f);

		menu.menuOptionText.color = optionColorDefault;

		yield return new WaitForSeconds(0.05f);

		menu.menuOptionText.color = optionColorPressed;

		yield return new WaitForSeconds(0.05f);

		menu.menuOptionText.color = optionColorDefault;

		yield return new WaitForSeconds(1f);

		switch (menu.menuOptionType)
		{
			case MenuType.MainMenuPlay:
				if (tokensOwned <= 0)
				{
					GoToTokenPurchasingScreen();
					menuPanel.SetActive(false);
					isInSecondaryMainMenu = true;
					ResetMenuOptions();
				}
				else
				{
					audioManager.PlaySound(AudioManager.SoundID.insertCoin);
					SceneManager.LoadScene(1);
				}
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
				if (tokensOwned <= 0)
				{
					GoToTokenPurchasingScreen();
					isInSecondaryMainMenu = true;
				}
				else
				{
					ResetMenuOptions();
					tokensPanel.SetActive(false);
					gameplayManager.ResetEverythingForANewGame();
				}
				break;
			case MenuType.GameplayLeaderboard:
				ChangeCurrentLeaderboard();
				SetMenuEnabled(true);
				break;
			case MenuType.GameplayQuit:
				SceneManager.LoadScene(0);
				break;
		}
	}

	public void UpdateLeaderboards()
	{
		leaderboardAllTimeNames = new List<ObscuredString>();
		leaderboardAllTimeScores = new List<ObscuredInt>();

		//TEST LIST DELETE LATER
		leaderboardAllTimeNames.Add("JOE");
		leaderboardAllTimeNames.Add("MAC");
		leaderboardAllTimeNames.Add("ROX");
		leaderboardAllTimeNames.Add("DAN");
		leaderboardAllTimeNames.Add("BOB");
		leaderboardAllTimeNames.Add("JIL");
		leaderboardAllTimeNames.Add("NED");
		leaderboardAllTimeNames.Add("REN");
		leaderboardAllTimeNames.Add("LIL");
		leaderboardAllTimeNames.Add("BEN");

		leaderboardAllTimeScores.Add(321);
		leaderboardAllTimeScores.Add(310);
		leaderboardAllTimeScores.Add(254);
		leaderboardAllTimeScores.Add(210);
		leaderboardAllTimeScores.Add(130);
		leaderboardAllTimeScores.Add(93);
		leaderboardAllTimeScores.Add(83);
		leaderboardAllTimeScores.Add(66);
		leaderboardAllTimeScores.Add(8);
		leaderboardAllTimeScores.Add(2);

		leaderboardMonthlyNames = new List<ObscuredString>();
		leaderboardMonthlyScores = new List<ObscuredInt>();

		//TEST LIST DELETE LATER
		leaderboardMonthlyNames.Add("BOI");
		leaderboardMonthlyNames.Add("TOM");
		leaderboardMonthlyNames.Add("NED");
		leaderboardMonthlyNames.Add("DAN");
		leaderboardMonthlyNames.Add("LAE");
		leaderboardMonthlyNames.Add("BAE");
		leaderboardMonthlyNames.Add("POP");
		leaderboardMonthlyNames.Add("QWO");
		leaderboardMonthlyNames.Add("JIO");
		leaderboardMonthlyNames.Add("ASD");

		leaderboardMonthlyScores.Add(143);
		leaderboardMonthlyScores.Add(142);
		leaderboardMonthlyScores.Add(140);
		leaderboardMonthlyScores.Add(138);
		leaderboardMonthlyScores.Add(134);
		leaderboardMonthlyScores.Add(131);
		leaderboardMonthlyScores.Add(130);
		leaderboardMonthlyScores.Add(123);
		leaderboardMonthlyScores.Add(112);
		leaderboardMonthlyScores.Add(101);

		leaderboardWeeklyNames = new List<ObscuredString>();
		leaderboardWeeklyScores = new List<ObscuredInt>();

		//TEST LIST DELETE LATER
		leaderboardWeeklyNames.Add("LOI");
		leaderboardWeeklyNames.Add("ANA");
		leaderboardWeeklyNames.Add("REI");
		leaderboardWeeklyNames.Add("JON");
		leaderboardWeeklyNames.Add("LUI");
		leaderboardWeeklyNames.Add("VIV");
		leaderboardWeeklyNames.Add("REO");
		leaderboardWeeklyNames.Add("FIO");
		leaderboardWeeklyNames.Add("DAN");
		leaderboardWeeklyNames.Add("POT");

		leaderboardWeeklyScores.Add(43);
		leaderboardWeeklyScores.Add(42);
		leaderboardWeeklyScores.Add(40);
		leaderboardWeeklyScores.Add(38);
		leaderboardWeeklyScores.Add(34);
		leaderboardWeeklyScores.Add(31);
		leaderboardWeeklyScores.Add(30);
		leaderboardWeeklyScores.Add(23);
		leaderboardWeeklyScores.Add(12);
		leaderboardWeeklyScores.Add(5);
	}

	public void ChangeCurrentLeaderboard()
	{
		audioManager.PlaySound(AudioManager.SoundID.menuOptionSelect);
		leaderboardToShow++;
		if (leaderboardToShow >= 3)
		{
			leaderboardToShow = 0;
		}
		UpdateLeaderboardDisplay();
	}

	public void ResetLeaderboardDisplay()
	{
		leaderboardTitleText.text = "";
		leaderboardNamesText.text = "";
		leaderboardPositionsText.gameObject.SetActive(false);
		leaderboardScoresText.text = "";
	}

	public void UpdateLeaderboardDisplay()
	{
		List<ObscuredString> leaderboardNames = new List<ObscuredString>();
		List<ObscuredInt> leaderboardScores = new List<ObscuredInt>();

		switch (leaderboardToShow)
		{
			case 0:
				leaderboardTitleText.text = "WEEKLY";
				leaderboardNames = leaderboardWeeklyNames;
				leaderboardScores = leaderboardWeeklyScores;
				break;
			case 1:
				leaderboardTitleText.text = "MONTHLY";
				leaderboardNames = leaderboardMonthlyNames;
				leaderboardScores = leaderboardMonthlyScores;
				break;
			case 2:
				leaderboardTitleText.text = "ALL TIME";
				leaderboardNames = leaderboardAllTimeNames;
				leaderboardScores = leaderboardAllTimeScores;
				break;
		}

		leaderboardPositionsText.gameObject.SetActive(true);
		leaderboardNamesText.text = "";
		leaderboardScoresText.text = "";

		for (int i = 0; i < 10; i++)
		{
			leaderboardNamesText.text += leaderboardNames[i] + "\n";
			leaderboardScoresText.text += leaderboardScores[i] + "\n";
		}
	}

	void GoToTokenPurchasingScreen()
	{
		if (mainMenuManager != null)
		{
			mainMenuManager.wenTitle.SetActive(false);
		}
		ResetLeaderboardDisplay();
		purchaseTokensPanel.SetActive(true);
		tokensPanel.SetActive(true);
		tokenCurrentPriceText.text = currentTokenPrice.ToString("0.0") + " NFTL";
		nftlCurrentOwnedText.text = nftlOwned.ToString("0.0") + " NFTL";
	}

	void PurchaseToken()
	{
		if (nftlOwned >= currentTokenPrice)
		{
			nftlOwned -= currentTokenPrice;
			GainToken();
			GoToTokenPurchasingScreen();
			TokenPurchaseAnim();
		}
		else
		{
			ErrorMessage("Not Enough NFTL!");
		}
	}

	void ErrorMessage(string message)
	{
		audioManager.PlaySound(AudioManager.SoundID.projectileHit);

		StopErrorMessage();

		errorMessageText.text = message;

		currentErrorMessageCoroutine = StartCoroutine(PlayErrorMessage());
	}

	IEnumerator PlayErrorMessage()
	{
		yield return new WaitForSeconds(3);

		errorMessageText.text = "";
	}

	void StopErrorMessage()
	{
		errorMessageText.text = "";

		if (currentErrorMessageCoroutine != null)
		{
			StopCoroutine(currentErrorMessageCoroutine);
		}
	}

	void TokenPurchaseAnim()
	{
		if (currentTokenPurchaseAnimationCoroutine != null)
		{
			StopCoroutine(currentTokenPurchaseAnimationCoroutine);
		}

		currentTokenPurchaseAnimationCoroutine = StartCoroutine(PlayTokenPurchaseAnim());
	}

	IEnumerator PlayTokenPurchaseAnim()
	{
		tokensPanelRect.anchoredPosition = tokenPanelStartPosition;

		float yStart = tokenPanelStartPosition.y;
		float yFinish = tokenPanelStartPosition.y + 0.2f;

		Tween<float> moveTween = new Tween<float>(yStart, yFinish, tokenPurchaseAnimSpeed, tokenPurchaseAnimTweenEaseType);
		while (!moveTween.IsEnded())
		{
			yield return new WaitForEndOfFrame();
			tokensPanelRect.anchoredPosition = new Vector2(tokensPanelRect.anchoredPosition.x, moveTween.Update(Time.deltaTime));
		}

		moveTween = new Tween<float>(yFinish, yStart, tokenPurchaseAnimSpeed, tokenPurchaseAnimTweenEaseType);
		while (!moveTween.IsEnded())
		{
			yield return new WaitForEndOfFrame();
			tokensPanelRect.anchoredPosition = new Vector2(tokensPanelRect.anchoredPosition.x, moveTween.Update(Time.deltaTime));
		}

		tokensPanelRect.anchoredPosition = tokenPanelStartPosition;
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
	GameplayLeaderboard,
}
