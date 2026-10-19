using System;
using System.Collections;
using UnityEngine;

public class TextTrigger : MonoBehaviour
{
    public string displayText;
    [Tooltip("time it takes until the fade out starts, 0 for no auto fade")]
    public float timeUntilFadeOut;
    [Tooltip("duration of the fade when the text is removed")]
    public float fadeTime;
    public bool oneUse;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }
    private void FixedUpdate()
    {
    }

    // Update is called once per frame
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            TextManager.Instance.DisplayText(displayText);
            if (timeUntilFadeOut > 0)
            {
                Debug.Log("waiting for fade");
                StartCoroutine(WaitForFade(timeUntilFadeOut));
            }
        }
    }

    IEnumerator WaitForFade(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        StartCoroutine(TextManager.Instance.TextFadeOut(fadeTime));
        if (oneUse) { gameObject.SetActive(false); }
    }
}
