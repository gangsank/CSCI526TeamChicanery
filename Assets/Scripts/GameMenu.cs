using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameMenu : MonoBehaviour
{
    //private bool active = false;
    const int mainMenuIndex = 0;
    public Button back;
    public TextMeshProUGUI text;

    private List<string> stages = new List<string>{
        "Course1-1", "Course1-2", "Course1-3", "Course1-4", "Course1-5", "Course1-6",
        "Course2-1", "Course2-2", "Course2-3", "Course2-4", "Course2-5", "Course2-6",
        "Course3-1", "Course3-2", "Course3-3", "Course3-4", "Course3-5", "Course3-6",
        "MixedCourse"
    };

    public void Show(string msg)
    {
        text.text = msg;
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
    }

    public void Hide()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Hide();
    }

    public void Exit()
    {
        SceneManager.LoadScene(mainMenuIndex);
        Hide();
    }

    public void Next()
    {
        string curScene = SceneManager.GetActiveScene().name;
        int curIndex = stages.IndexOf(curScene);
        if (curIndex + 1 < stages.Count)
        {
            SceneManager.LoadScene(stages[curIndex+1]);
        }
    }

}
