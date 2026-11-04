using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class Door : MonoBehaviour
{
    public static List<Door> All = new List<Door>();
    public Vector3 openOffset = new Vector3(0f, 3f, 0f);
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
        Vector3 target = pressCount > 0 || open ? closedPos + openOffset : closedPos;
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, target, openSpeed * Time.deltaTime);
    }
}


