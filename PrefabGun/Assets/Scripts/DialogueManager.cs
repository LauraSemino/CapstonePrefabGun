using Ink.Runtime;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    public static DialogueController Instance { get; private set; }

    public GameObject diaPanel;
    public TMP_Text diaText;
    public float typeySpeed = 0.05f;
    public Story currStory = null;
    private bool isTyping;
    public float lineDelay;

    private void Awake()
    {
        Instance = this;
        diaPanel.SetActive(false);
    }

    public void StartDia(TextAsset ink)
    {
        currStory = new Story(ink.text);
        diaPanel.SetActive(true);
        ContinueDia();
    }

    void ContinueDia()
    {

        if (currStory.canContinue)
        {
            string line = currStory.Continue();
            StartCoroutine(TypeDia(line));

        }
        else
        {
            EndDia();
        }
    }

    public void EndDia()
    {
        diaPanel.SetActive(false);
        currStory = null;
    }

    IEnumerator TypeDia(string line)
    {
        isTyping = true;
        diaText.text = "";
        foreach (char c in line)
        {
            diaText.text += c;
            yield return new WaitForSeconds(typeySpeed);
        }
        diaText.text = line;
        isTyping = false;
        yield return new WaitForSeconds(lineDelay);
        ContinueDia();
    }
}
