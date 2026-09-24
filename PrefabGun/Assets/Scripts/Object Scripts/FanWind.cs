using UnityEngine;

public class FanWind : MonoBehaviour
{
    public float playerForce = 25f;
    public float upwardForce = 15f;
    public float windness = 15f;

    public void OnTriggerStay(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.AddForce(Vector3.up * windness, ForceMode.Force);
        }

        PlayerControl player = other.GetComponent<PlayerControl>();

        if (player != null)
        {
            player.Explode(Vector3.up * windness * 0.075f);
        }
    }
}
