using TMPro;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
	public static MainMenuManager I;

	public MenuManager menuManager;
	[Space]
	public GameObject wenTitle;
	public GameObject howToPlayPanel;
	public GameObject leaderboardsPanel;
	public GameObject aboutPanel;
	public TextMeshProUGUI statusText;
	public TextMeshProUGUI balanceText;

	private void Awake()
	{
		I = this;
		menuManager.SetMenuEnabled(false);
		statusText.gameObject.SetActive(true);
	}

	public void GoToHowToPlayScreen()
	{
		wenTitle.SetActive(false);
		howToPlayPanel.SetActive(true);
	}

	public void GoToLeaderboardsScreen()
	{
		wenTitle.SetActive(false);
		leaderboardsPanel.SetActive(true);
	}

	public void GoToAboutScreen()
	{
		wenTitle.SetActive(false);
		aboutPanel.SetActive(true);
	}

	public void GoBack()
	{
		wenTitle.SetActive(true);
		howToPlayPanel.SetActive(false);
		leaderboardsPanel.SetActive(false);
		aboutPanel.SetActive(false);
	}

	public static void Initialize()
	{
		I.menuManager.SetMenuEnabled(true);
		SetStatus("");
	}

	public static void SetStatus(string text)
	{
		I.statusText.text = text.ToUpper();
	}

	public static void SetTokenBalance(uint balance)
	{
		I.balanceText.text = balance.ToString("000");
	}
}
