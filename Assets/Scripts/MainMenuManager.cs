using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public MenuManager menuManager;
    [Space]
    public GameObject wenTitle;
    public GameObject howToPlayPanel;
    public GameObject leaderboardsPanel;
    public GameObject aboutPanel;

    private void Start()
    {
        menuManager.TurnOnMenu();
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
}
