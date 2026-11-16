
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{
    [Header("Objects")]
    public float sinkMass = 5f;
    public float buoyancy = 2f;
    public float floatRange = 1f;
    public float bobHeight = 0.05f;
    public float bobSpeed = 2f;
    public float drag = 2f;

    [Header("Player")]
    public float playerVerticalDrag = 5f;

    Collider zone;
    List<Rigidbody> bodies = new List<Rigidbody>();

    void Awake()
    {
        zone = GetComponent<Collider>();
        zone.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb == null || other.isTrigger) return;
        if (!bodies.Contains(rb))
        {
            bodies.Add(rb);
        }
    }

    void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null)
        {
            bodies.Remove(rb);
        }
    }

    void FixedUpdate()
    {
        float surfaceY = zone.bounds.max.y;
        foreach (Rigidbody rb in bodies)
        {
            if (rb == null || !rb.gameObject.activeInHierarchy || rb.isKinematic)
                continue;

            // Player
            if (rb.GetComponentInParent<PlayerControl>() != null)
            {
                Vector3 velocity = rb.linearVelocity;
                velocity.y *= 1f - playerVerticalDrag * Time.fixedDeltaTime;
                rb.linearVelocity = velocity;
                continue;
            }

            // Water resistance
            rb.linearVelocity *= 1f - drag * Time.fixedDeltaTime;
            rb.angularVelocity *= 1f - drag * Time.fixedDeltaTime;

            // Floating objects
            if (rb.mass < sinkMass)
            {
                float bob = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
                float depth = surfaceY - rb.worldCenterOfMass.y + bob;

                float submerged = Mathf.Clamp01(0.5f + depth / floatRange);

                float upForce = -Physics.gravity.y * rb.mass * buoyancy * submerged;

                rb.AddForce(Vector3.up * upForce);
            }
        }
    }
}
