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

    private void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            Debug.Log(timer);
        }
        else if (timer < 0)
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
        //tryna make it fade out properly but it only instantly removes it currently
        popUp.alpha = Mathf.Lerp(255,0,timer);
        yield return new WaitForSeconds(timeToFade);
    }
}
