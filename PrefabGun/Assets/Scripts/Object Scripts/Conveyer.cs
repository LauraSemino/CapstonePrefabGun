using System;
using UnityEngine;

public class Conveyer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] float speed;
    // Update is called once per frame
    void Update()
    {

    }
    private void OnCollisionStay(Collision collision)
    {
        collision.rigidbody.AddForce((speed*collision.rigidbody.mass*(-transform.right)) - collision.rigidbody.linearVelocity, ForceMode.VelocityChange);
        //collision.rigidbody.linearVelocity = (speed * -transform.right);
    }
}
