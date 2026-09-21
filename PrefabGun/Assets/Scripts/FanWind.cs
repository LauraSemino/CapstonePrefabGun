using UnityEngine;

public class FanWind : MonoBehaviour
{
    public float windness = 15f;

    public void OnTriggerStay(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();

        if (rb !=null)
        {
            rb.AddForce(Vector3.up * windness, ForceMode.Force);
        }
    }
}
