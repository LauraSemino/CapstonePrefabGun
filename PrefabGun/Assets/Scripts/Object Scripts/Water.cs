
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
    public float playerJumpMult;

    [Header("Drag")]
    public float verticalDrag = 5f;
    public float horizontalDrag = 2f;
    public float angularDrag = 2f;

    Collider zone;
    List<Rigidbody> bodies = new List<Rigidbody>();

    void Awake()
    {
        zone = GetComponent<Collider>();
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


            if (rb.GetComponent<FanWind>() != null)
            {
                FanWind fw = rb.GetComponent<FanWind>();
                fw.FanState();
            }

            // Player
            if (rb.GetComponentInParent<PlayerControl>() != null)
            {
                Vector3 velocity = rb.linearVelocity;
                if ( velocity.y > 0)
                {
                    velocity.y += (playerJumpMult * Time.deltaTime) *1.1f;
                }
                else
                {
                    velocity.y *= 1f - verticalDrag * Time.fixedDeltaTime;
                    velocity.x *= 1f - horizontalDrag * Time.fixedDeltaTime;
                    velocity.z *= 1f - horizontalDrag * Time.fixedDeltaTime;
                }
                rb.linearVelocity = velocity;
                continue;
            }
            else
            {
                // Water resistance
                Vector3 lv = rb.linearVelocity;
                lv.y *= 1f - verticalDrag * Time.fixedDeltaTime;
                lv.x *= 1f - horizontalDrag * Time.fixedDeltaTime;
                lv.z *= 1f - horizontalDrag * Time.fixedDeltaTime;
                rb.linearVelocity = lv;
                rb.angularVelocity *= 1f - angularDrag * Time.fixedDeltaTime;
            }

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
