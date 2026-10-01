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
        collision.rigidbody.AddForce((speed*collision.rigidbody.mass*(-transform.right)) - new Vector3(collision.rigidbody.linearVelocity.x,0,collision.rigidbody.linearVelocity.z) , ForceMode.VelocityChange);
        Debug.Log("applying force: " + ((speed * collision.rigidbody.mass * (-transform.right)) - collision.rigidbody.linearVelocity));
        //collision.rigidbody.linearVelocity = (speed * -transform.right);
    }
}
