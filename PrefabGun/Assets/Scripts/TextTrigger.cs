using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class TextTrigger : MonoBehaviour
{
    public enum DisappearMode { Time, Distance}
    public DisappearMode mode = DisappearMode.Time; 
    public string displayText;

    [Header("Timer Settings")]
    [Tooltip("time it takes until the fade out starts, 0 for no auto fade")]
    public float timeUntilFadeOut;
    [Tooltip("duration of the fade when the text is removed")]
    public float fadeTime;
    [Tooltip("Waits this long before the text can start displaying")]
    public float delayActivation;

    [Header("Distance Settings")]
    public float distanceThreshold;

    [Header("General Settings")]
    public bool oneUse;

    bool hasUsed;
    bool doDistanceCheck;
    GameObject player;
    float delayTimer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponent<MeshRenderer>().enabled = false;
        hasUsed = false;
    }
    private void Update()
    {
        if(doDistanceCheck)
        {
            Debug.Log((gameObject.transform.position - player.transform.position).magnitude);
            if (distanceThreshold < (gameObject.transform.position - player.transform.position).magnitude)
            {
                StartCoroutine(TextManager.Instance.WaitForFade(0,fadeTime));
                doDistanceCheck = false;
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" && !hasUsed && !TextManager.Instance.displaying)
        {
            delayTimer = 0;   
        }
     }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player" && !hasUsed && !TextManager.Instance.displaying)
        {         
            if (delayTimer < delayActivation)
            {
                //happens instantly for some reason?
                delayTimer += Time.deltaTime;
                Debug.Log(delayTimer);
            }
            if(delayTimer >= delayActivation)
            {
                TextManager.Instance.DisplayText(displayText);
                if (mode == DisappearMode.Time)
                {
                    //Debug.Log("waiting for fade");
                    StartCoroutine(TextManager.Instance.WaitForFade(timeUntilFadeOut, fadeTime));               
                }
                else if (mode == DisappearMode.Distance)
                {
                    doDistanceCheck = true;
                    player = other.gameObject;
                }
                if (oneUse) { hasUsed = true; }
            }
        }
    }


}
