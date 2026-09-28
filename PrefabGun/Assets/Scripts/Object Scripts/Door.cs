using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class Door : MonoBehaviour
{
    public void Open()
    {
        BoxCollider bc = gameObject.GetComponent<BoxCollider>();
        MeshRenderer sc = gameObject.GetComponent<MeshRenderer>();
        bc.enabled = false;
        sc.enabled = false;
    }

    public void Close()
    {
        BoxCollider bc = gameObject.GetComponent<BoxCollider>();
        MeshRenderer sc = gameObject.GetComponent<MeshRenderer>();
        bc.enabled = true;
        sc.enabled = true;
    }
}
