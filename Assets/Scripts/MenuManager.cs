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
	public List<Menu> menus;
	public int currentMenu;
	public int currentMenuOption;
	public List<TextMeshProUGUI> menuTexts;
	[Space]
	public RectTransform menuCursor;
	public GameObject menuPanel;
	public GameObject tokensPanel;
	public GameObject purchaseTokensPanel;
	public GameObject whiteFlash;
	public float menuCursorYOffset;
	[Space]
	public TextMeshProUGUI tokenAmountText;
	public Color32 tokenAmountDefaultColor;
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

	private MenuOption lastSelectedMenuOption;
	private bool canSelectMenuOptions;
	private float nftlOwned = 2000;
	private float currentTokenPrice = 1000;
	private uint tokensOwned;
	private InputState input = new InputState();

	void Start()
	{
		UpdateTokenAmount();
		ResetMenuOptions();
		SetSelectedMenuOption();
		UpdateLeaderboards();
		
		tokenPanelStartPosition = tokensPanelRect.anchoredPosition;
	}

	void Update()
	{
		InputReader.GetInput(input);

		if (currentMenu > 0 && input.PressedB)
		{
			audioManager.PlaySound(AudioManager.SoundID.batSwing);
			if (mainMenuManager != null)
			{
				mainMenuManager.GoBack();
			}
			purchaseTokensPanel.SetActive(false);
			UpdateLeaderboardDisplay();
			SetMenuEnabled(true);
			ChangeMenu(0);
			return;
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
		canSelectMenuOptions = enabled;
		menuPanel.SetActive(enabled);
		ResetMenuOptions();
	}

	void ChangeMenu(int menuToChangeTo)
	{
		currentMenu = menuToChangeTo;
		currentMenuOption = 0;
		canSelectMenuOptions = true;
		ResetMenuOptions();
		SetSelectedMenuOption();
	}

	void ChangeMenuOption(int menuOptionChange)
	{
		audioManager.PlaySound(AudioManager.SoundID.messagePopup);

		currentMenuOption = currentMenuOption + menuOptionChange;

		if (currentMenuOption > menus[currentMenu].menuOptions.Count - 1)
		{
			currentMenuOption = 0;
		}

		if (currentMenuOption < 0)
		{
			currentMenuOption = menus[currentMenu].menuOptions.Count - 1;
		}

		SetSelectedMenuOption();
	}

	void SetSelectedMenuOption()
	{
		menuCursor.anchoredPosition = new Vector2(menuCursor.anchoredPosition.x, menuTexts[currentMenuOption].rectTransform.anchoredPosition.y + menuCursorYOffset);
	}

	void ResetMenuOptions()
	{
		foreach (TextMeshProUGUI menuText in menuTexts) 
		{
			menuText.text = "";
		}

		for (int i = 0; i < menus[currentMenu].menuOptions.Count; i++)
		{
			menuTexts[i].text = menus[currentMenu].menuOptions[i].menuNameString;
		}

		if (tokensOwned <= 0 && currentMenu == 0)
		{
			menuTexts[0].text = "PURCHASE TOKENS";
		}

		errorMessageText.text = "";
	}

	public void GainTokens()
	{
		tokensOwned += 4;
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

	public void ShowTokenBalance(bool show)
	{
		tokensPanel.SetActive(show);
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
		MenuOption menuOption = menus[currentMenu].menuOptions[currentMenuOption];
		lastSelectedMenuOption = menuOption;
		audioManager.PlaySound(AudioManager.SoundID.menuOptionSelect);

		if (currentMenu == 0)
		{
			foreach (TextMeshProUGUI menuText in menuTexts)
			{
				menuText.text = "";
			}

			menuTexts[currentMenuOption].text = menuOption.menuNameString;

			if (menuOption.subMenuID == "MainMenuPlay" || menuOption.subMenuID == "GameplayTryAgain")
			{
				if (tokensOwned > 0)
				{
					audioManager.PlaySound(AudioManager.SoundID.insertCoin);
					SpendToken();
				}
				else
				{
					menuTexts[0].text = "PURCHASE TOKENS";
				}
			}

			whiteFlash.gameObject.SetActive(true);

			yield return new WaitForSeconds(0.05f);

			whiteFlash.gameObject.SetActive(false);

			yield return new WaitForSeconds(0.1f);

			menuTexts[currentMenuOption].color = optionColorPressed;

			yield return new WaitForSeconds(0.05f);

			menuTexts[currentMenuOption].color = optionColorDefault;

			yield return new WaitForSeconds(0.05f);

			menuTexts[currentMenuOption].color = optionColorPressed;

			yield return new WaitForSeconds(0.05f);

			menuTexts[currentMenuOption].color = optionColorDefault;

			yield return new WaitForSeconds(1f);

		}

		switch (menuOption.subMenuID)
		{
			case "MainMenuPlay":
				if (tokensOwned <= 0)
				{
					GoToTokenPurchasingScreen();
					ChangeMenu(1);
					ShowTokenBalance(true);
				}
				else
				{
					audioManager.PlaySound(AudioManager.SoundID.insertCoin);
					SceneManager.LoadScene(1);
				}
				break;
			case "MainMenuHowToPlay":
				mainMenuManager.GoToHowToPlayScreen();
				ShowTokenBalance(false);
				ChangeMenu(2);
				break;
			case "MainMenuLeaderboards":
				mainMenuManager.GoToLeaderboardsScreen();
				ShowTokenBalance(false);
				ChangeMenu(3);
				break;
			case "MainMenuQuit":
				Application.Quit();
				break;

			case "TokenMenuPurchase":
				PurchaseToken();
				ShowTokenBalance(true);
				break;
			case "TokenMenuBack":
				mainMenuManager.GoBack();
				ShowTokenBalance(true);
				purchaseTokensPanel.SetActive(false);
				ChangeMenu(0);
				break;

			case "HowToPlayMenuBack":
				mainMenuManager.GoBack();
				ShowTokenBalance(true);
				ChangeMenu(0);
				break;

			case "MainMenuLeaderboardMenuChange":
				ChangeCurrentLeaderboard();
				break;
			case "MainMenuLeaderboardMenuBack":
				mainMenuManager.GoBack();
				ShowTokenBalance(true);
				ChangeMenu(0);
				break;

			case "GameplayTryAgain":
				if (tokensOwned <= 0)
				{
					GoToTokenPurchasingScreen();
					ChangeMenu(1);
					ShowTokenBalance(true);
				}
				else
				{
					ResetMenuOptions();
					tokensPanel.SetActive(false);
					gameplayManager.ResetEverythingForANewGame();
					menuPanel.SetActive(false);
				}
				break;
			case "GameplayLeaderboard":
				ChangeCurrentLeaderboard();
				SetMenuEnabled(true);
				break;
			case "GameplayQuit":
				SceneManager.LoadScene(0);
				break;

			case "GameplayTokenPurchase":
				PurchaseToken();
				break;
			case "GameplayTokenBack":
				purchaseTokensPanel.SetActive(false);
				UpdateLeaderboardDisplay();
				ChangeMenu(0);
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
		leaderboardToShow++;
		if (leaderboardToShow >= 3)
		{
			leaderboardToShow = 0;
		}
		canSelectMenuOptions = true;
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
	}

	void PurchaseToken()
	{
		if (nftlOwned >= currentTokenPrice)
		{
			nftlOwned -= currentTokenPrice;
			GainTokens();
			GoToTokenPurchasingScreen();
			TokenPurchaseAnim();
		}
		else
		{
			ErrorMessage("Not Enough NFTL!");
		}

		canSelectMenuOptions = true;
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
public class Menu
{
	public string menuID;
	public List<MenuOption> menuOptions;
}

[Serializable]
public class MenuOption
{
	public string menuNameString;
	public string subMenuID;
}
