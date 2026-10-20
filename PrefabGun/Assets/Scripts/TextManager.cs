using System.Collections;
using TMPro;
using UnityEngine;

public class TextManager : MonoBehaviour
{
    public static TextManager Instance { get { return _instance; } }
    private static TextManager _instance;

    public TextMeshProUGUI popUp;
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
        popUp.text = text;
        popUp.color = Color.white;
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
            Debug.Log(timer);
            
        }     
    }
}
