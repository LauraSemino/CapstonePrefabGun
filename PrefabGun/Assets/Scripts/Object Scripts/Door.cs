using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class Door : MonoBehaviour
{
    public static List<Door> All = new List<Door>();
    public int openOffset = 4;
    public float openSpeed = 4f;
    public int pressCount = 0;
    public bool open;
    public int frequency;

    Vector3 closedPos;

    void Awake() 
    {
        closedPos = transform.localPosition; 
    }
    void OnEnable() 
    {
        All.Add(this); 
    }
    void OnDisable() 
    { 
        All.Remove(this); 
    }

    void Update()
    {
        Vector3 target = pressCount > 0 || open ? closedPos + (transform.up * openOffset) : closedPos;
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, target, openSpeed * Time.deltaTime);
    }
}


