using UnityEngine;

public class ExplosiveBarrel : MonoBehaviour
{
    public float boomRadius = 5f;
    public float boomForce = 50f;
    public float playerForce = 25f;
    public float upwardForce = 15f;

    bool exploded = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (exploded) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            Explode();
            return;
        }

        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();

        if (rb != null && collision.relativeVelocity.magnitude > 2f)
        {
            Explode();
        }
    }

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

            PlayerControl player = obj.GetComponent<PlayerControl>();

            if (player != null)
            {
                Vector3 direction = (player.transform.position - transform.position).normalized;
                direction.y = 0;

                player.Explode(direction * playerForce + Vector3.up * upwardForce);
            }
        }

        Destroy(gameObject);
    }
}
