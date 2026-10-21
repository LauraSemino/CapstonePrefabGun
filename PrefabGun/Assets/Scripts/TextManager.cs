using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.ShaderGraph.Serialization;
using UnityEngine;

public class TextManager : MonoBehaviour
{
    public static TextManager Instance { get { return _instance; } }
    private static TextManager _instance;
    public TextMeshProUGUI popUp;
    public bool displaying;
   // float timer;

    private void Start()
    {
        _instance = this;
    }

    private void Update()
    {
      
    }

    public void DisplayText(string text)
    {
        if(displaying == false)
        {
            popUp.text = text;
            popUp.alpha = 1;
            displaying = true;
        }
    }

    public IEnumerator TextFadeOut(float timeToFade)
    {        
        float timer = timeToFade;
        //tryna make it fade out properly but it only instantly removes it currently
        while (timer > 0) 
        {      
            popUp.alpha = (timer/timeToFade);
            yield return null;
            timer = Mathf.Clamp(timer - Time.deltaTime, 0, 999);
            //Debug.Log(timer);            
        }
        if (timer == 0)
        {
            displaying = false;
        }
    }
    public IEnumerator WaitForFade(float seconds, float fadeTime)
    {
        Debug.Log("running");
        yield return new WaitForSeconds(seconds);
        StartCoroutine(TextFadeOut(fadeTime));

    }
}
