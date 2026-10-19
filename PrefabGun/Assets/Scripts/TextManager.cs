using System.Collections;
using TMPro;
using UnityEngine;

public class TextManager : MonoBehaviour
{
    public static TextManager Instance { get { return _instance; } }
    private static TextManager _instance;

    public TextMeshProUGUI popUp;
    float timer;

    private void Start()
    {
        _instance = this;
    }
    private void FixedUpdate()
    {
        if (timer > 0)
        {
            timer--;
        }
        else if(timer < 0)
        {
            timer = 0;
        }
    }

    public void DisplayText(string text)
    {
        popUp.text = text;
        popUp.color = Color.white;
    }

    public IEnumerator TextFadeOut(float timeToFade)
    {
        Debug.Log("fading");
        timer = timeToFade;
        //tryna make it fade out properly but it doesnt 
        popUp.color = new Color(255, 255, 255, Mathf.Lerp(0,255,timer));
        yield return new WaitForSeconds(timeToFade);
    }
}
