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
    private void OnTriggerStay(Collider collision)
    {
        Rigidbody otherRb = collision.gameObject.GetComponent<Rigidbody>();

        if (otherRb == null)
            return;

        if (otherRb.GetComponent<PlayerControl>())
        {
        otherRb.AddForce(-transform.right * (speed*10), ForceMode.Acceleration);
        }
        otherRb.AddForce(-transform.right * speed, ForceMode.Acceleration);
    }
}
