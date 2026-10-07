using System.Collections;
using UnityEngine;

public class ExplosiveBarrel : MonoBehaviour
{
    public float boomRadius = 5f;
    public float boomForce = 50f;
    public float respawnTime = 5f;

    bool exploded = false;

    Vector3 startPos;
    Quaternion startRot;
    Rigidbody barrrelRb;

    void Awake()
    {
        startPos = transform.position;
        startRot = transform.rotation;
        barrrelRb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (exploded) return;

        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();

        if (rb != null && collision.relativeVelocity.magnitude > 2f)
        {
            Explode();
        }
    }

    // Go boom
    void Explode()
    {
        exploded = true;

        Collider[] objects = Physics.OverlapSphere(transform.position, boomRadius);

        foreach (Collider obj in objects)
        {
            Rigidbody objectRb = obj.GetComponent<Rigidbody>();

            if (objectRb != null)
            {
                objectRb.AddExplosionForce(boomForce, transform.position, boomRadius, 1f, ForceMode.Impulse);
            }
        }

        ObjectData data = GetComponent<ObjectData>();

        if (data != null && data.createdByPlayer)
        {
            // Player-placed barrel gives back the $$ (Budget)
            PrefabGun.instance.Refund(2);
            Destroy(gameObject);
        }
        else
        {
            // Original barrel should always respawn
            StartCoroutine(RespawnRoutine());
        }
    }

    IEnumerator RespawnRoutine()
    {
        SetVisible(false);

        yield return new WaitForSeconds(respawnTime);

        transform.SetPositionAndRotation(startPos, startRot);
        barrrelRb.isKinematic = true;
           

        SetVisible(true);
        exploded = false;
    }

    void SetVisible(bool visible)
    {
        Renderer r = GetComponent<Renderer>();
            r.enabled = visible;

        Collider c = GetComponent<Collider>();
            c.enabled = visible;

        if (!visible)
            barrrelRb.isKinematic = true;
    }
}
