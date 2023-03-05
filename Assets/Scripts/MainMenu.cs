using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject tutorialPanel;
    public GameObject gameMenu;
    public List<GameObject> levelPanels;

    // Start is called before the first frame update
    void Start()
    {
        ShowMainMenu();
        DontDestroyOnLoad(gameMenu);
    }

    public void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        foreach (GameObject panel in levelPanels)
        {
            panel.SetActive(false);
        }
        tutorialPanel.SetActive(false);
    }

    public void ShowTutorial()
    {
        tutorialPanel.SetActive(true);
        mainMenu.SetActive(false);
    }

    public void ShowLevelPanel(int level)
    {
        mainMenu.SetActive(false);
        levelPanels[level - 1].SetActive(true);
    }

    public void LoadScene(string name)
    {
        SceneManager.LoadScene(name);
    }

}
