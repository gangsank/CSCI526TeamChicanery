using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Tutorial : MonoBehaviour
{
    static IEnumerator displayMessageAction;

    [TextArea] public string message;
    [SerializeField] private TextMeshProUGUI gui;
    private float fadeInTime = 1f;
    private float fadeOutTime = 2f;
    private float displayTime = 7f;
    private bool isShown = false;
   
    private void Start()
    { 
        gui.CrossFadeAlpha(0, 0, true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isShown && other.CompareTag(Config.Tag.Player))
        {
            if (Tutorial.displayMessageAction != null)
            {
                StopCoroutine(Tutorial.displayMessageAction);
            }
            Tutorial.displayMessageAction = DisplayMessage();
            StartCoroutine(Tutorial.displayMessageAction);
        }
    }

    private IEnumerator DisplayMessage()
    {
        isShown = true;
        gui.text = message;
        gui.CrossFadeAlpha(1, fadeInTime, true);
        yield return new WaitForSeconds(displayTime + fadeInTime);
        gui.CrossFadeAlpha(0, fadeOutTime, true);
        Tutorial.displayMessageAction = null;
    }

}
