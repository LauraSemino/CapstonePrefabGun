using System;
using UnityEngine;

public class Trampoline : MonoBehaviour
{
    [SerializeField] float bounciness;

    Vector3 savedObjectVelocity;
    Rigidbody rb;
    PlayerControl player;
    private void OnTriggerEnter(Collider other)
    {
        rb = other.GetComponent<Rigidbody>();
        player = other.GetComponent<PlayerControl>();

        if (rb != null && player == null)
        {
            //Vector3 BounceForce = -rb.linearVelocity.y * transform.up * bounciness;
            Vector3 BounceForce = Vector3.Project(rb.linearVelocity, transform.up) * bounciness;
            //multiply by rotation of trampoline here            
            rb.AddForce(BounceForce * rb.mass, ForceMode.Impulse);

            Debug.Log("bouncing object: "+ BounceForce);
        }
        if (player != null && !player.isGrounded)
        {
            Vector3 BounceForce = Vector3.Project(player.Velocity, transform.up) * bounciness;
            //multiply by rotation of trampoline here
            player.PushPlayer(BounceForce);

            Debug.Log("bouncing player: " + BounceForce);
        }
    }

    //below is a bunch of bullshit i tried at some point
    /*private void OnTriggerStay(Collider other)
    {
        if (rb != null && player == null)
        {
            savedObjectVelocity = rb.linearVelocity;
            //Debug.Log(savedObjectVelocity);
        }
        if (player != null && !player.isGrounded)
        {
            savedObjectVelocity = player.Velocity;
            //Debug.Log(savedObjectVelocity);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (rb != null)
        {
            Debug.Log(savedObjectVelocity);
            Vector3 BounceForce = new Vector3(0, -savedObjectVelocity.y * bounciness, 0);
            //multiply by rotation of trampoline here
            rb.AddForce(BounceForce * rb.mass, ForceMode.Impulse);
            Debug.Log("bouncing object: " + BounceForce);
        }
    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Vector3 BounceForce = new Vector3(0, -savedObjectVelocity.y * bounciness, 0);
        //multiply by rotation of trampoline here
        player.PushPlayer(BounceForce);
        Debug.Log("bouncing object: " + BounceForce);
    }*/
}
