using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using UnityEngine.SceneManagement;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.Text;

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
	public Color32 tokenAmountEmptyColor;
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
	private InputState input = new InputState();

	uint ArcadeTokens { get { return NiftyUsers.me != null ? NiftyUsers.me.arcadeTokenBalance : 0; } }

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

		if (ArcadeTokens <= 0 && currentMenu == 0)
		{
			menuTexts[0].text = "PURCHASE TOKENS";
		}

		errorMessageText.text = "";
	}

	public IEnumerator GainTokens()
	{
		yield return SubmitTokenPurchase();

		yield return RefreshArcadeBalance();

		UpdateTokenAmount();
		audioManager.PlaySound(AudioManager.SoundID.gainPoint);
		TokenPurchaseAnim();

		canSelectMenuOptions = true;
		if (mainMenuManager != null)
		{
			mainMenuManager.GoBack();
		}
		else
		{
			UpdateLeaderboardDisplay();
		}
		ShowTokenBalance(true);
		purchaseTokensPanel.SetActive(false);
		ChangeMenu(0);
	}

	public void SpendToken()
	{
		//tokensOwned -= 1;
		UpdateTokenAmount();
	}

	public void SetTokenBalance(uint balance)
	{
		UpdateTokenAmount();
		ShowTokenBalance(true);
	}

	public void ShowTokenBalance(bool show)
	{
		tokensPanel.SetActive(show);
	}

	public void UpdateTokenAmount()
	{
		tokenAmountText.color = tokenAmountDefaultColor;
		if (ArcadeTokens <= 0)
		{
			tokenAmountText.color = tokenAmountEmptyColor;
		}
		tokenAmountText.text = ArcadeTokens.ToString("000");
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
				if (ArcadeTokens > 0)
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
			if (ArcadeTokens <= 0)
			{
				GoToTokenPurchasingScreen();
				ChangeMenu(1);
				ShowTokenBalance(true);
			}
			else
			{
				yield return StartGame();
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
			Launcher.Logout();
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
			if (ArcadeTokens <= 0)
			{
				GoToTokenPurchasingScreen();
				ChangeMenu(1);
				ShowTokenBalance(true);
			}
			else
			{
				ResetMenuOptions();
				tokensPanel.SetActive(false);
				menuPanel.SetActive(false);
				yield return StartGame();
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


	private IEnumerator StartGame()
	{
		UnityWebRequest www = null;
		Dictionary<string, string> headers = new Dictionary<string, string>
		{
			{ "authorizationToken", NiftyUsers.GetMyAuthorization() },
		};
		yield return Utils.PostRequest("https://odgwhiwhzb.execute-api.us-east-1.amazonaws.com/prod/matches/wen-game/start", null, (w) => www = w, headers);

		string matchId = null;
		int updateTicket = 0;
		if (www.result != UnityWebRequest.Result.Success)
		{
			print("Failed to Insert Token");
		}
		else
		{
			try
			{
				JObject info = JObject.Parse(www.downloadHandler.text);
				matchId = info["id"] != null ? (string)info["id"] : null;
				updateTicket = info["ticket"] != null ? (int)info["ticket"] : 0;
			}
			catch
			{
				Debug.Log("Failed to update Arcade Token balance");
			}
		}
		yield return RefreshArcadeBalance();
		if (matchId != null && updateTicket != 0)
		{
			EventController.AddMatchStart(matchId, updateTicket);
			UIVersion.SetSession(EventController.GetMatchShortId());
			yield return new WaitForSeconds(0.25f);
			SceneManager.LoadScene(1);
		}
		else
		{
			SetMenuEnabled(true);
		}
	}

	public IEnumerator RefreshArcadeBalance()
	{
		UnityWebRequest www = null;
		Dictionary<string, string> headers = new Dictionary<string, string>
		{
			{ "authorizationToken", NiftyUsers.GetMyAuthorization() },
		};
		yield return Utils.GetRequest("https://odgwhiwhzb.execute-api.us-east-1.amazonaws.com/prod/accounts/account/inventory?id=arcade-token", (w) => www = w, headers);

		uint arcadeBalance = 0;
		if (www.result != UnityWebRequest.Result.Success)
		{
			print("Failed to fetch inventory");
			yield break;
		}
		else
		{
			try
			{
				JObject inventory = JObject.Parse(www.downloadHandler.text);
				arcadeBalance = inventory["balance"] != null ? (uint)inventory["balance"] : 0;
				NiftyUsers.me.SetArcadeBalance(arcadeBalance);
			}
			catch
			{
				Debug.Log("Failed to update Arcade Token balance");
			}
		}
	}

	public IEnumerator SubmitTokenPurchase()
	{
		UnityWebRequest www = null;
		Dictionary<string, string> headers = new Dictionary<string, string>
		{
			{ "authorizationToken", NiftyUsers.GetMyAuthorization() },
		};
		byte[] data = Encoding.ASCII.GetBytes(@"{
			'id': 'arcade-token-four-pack',
			'currency': 'nftl',
			'price': 1000
		}".Replace('\'', '"'));
		yield return Utils.PostRequest("https://odgwhiwhzb.execute-api.us-east-1.amazonaws.com/prod/marketplace/product/purchase", data, (w) => www = w, headers);

		if (www.result != UnityWebRequest.Result.Success)
		{
			print("Failed to fetch inventory");
			yield break;
		}
		else
		{
			print(www.downloadHandler.text);
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
		ResetLeaderboardDisplay();
		purchaseTokensPanel.SetActive(true);
		tokensPanel.SetActive(true);
	}

	void PurchaseToken()
	{
		StartCoroutine(GainTokens());
		GoToTokenPurchasingScreen();
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
