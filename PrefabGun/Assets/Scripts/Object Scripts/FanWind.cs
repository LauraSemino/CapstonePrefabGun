using UnityEngine;

public class FanWind : MonoBehaviour
{
    public float upwardForce = 15f;
    public float windness = 15f;

    public void OnTriggerStay(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.AddForce(transform.up * windness, ForceMode.Force);
        }
    }
}
