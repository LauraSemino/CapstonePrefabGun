using System;
using UnityEngine;

public class Trampoline : MonoBehaviour
{
    [SerializeField] float bounciness;
    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        PlayerControl player = other.GetComponent<PlayerControl>();

        if (rb != null && player == null)
        {
            Vector3 BounceForce = new Vector3(rb.linearVelocity.x, -rb.linearVelocity.y * bounciness, rb.linearVelocity.z);
            //multiply by rotation of trampoline here
            rb.AddForce(BounceForce * rb.mass, ForceMode.Impulse);

            Debug.Log("bouncing object: "+ BounceForce);
        }
        if (player != null)
        {
            Vector3 BounceForce = new Vector3(player.Velocity.x, -player.Velocity.y * bounciness, player.Velocity.z);
            //multiply by rotation of trampoline here
            player.PushPlayer(BounceForce);

            Debug.Log("bouncing player: " + BounceForce);
        }
    }
}
