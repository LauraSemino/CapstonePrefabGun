using UnityEngine;

public class CollisionBreak : MonoBehaviour
{
    bool exploded = false;
    public bool activate = false;

    float velMin = 8f;

    public Door door;
    private void OnCollisionEnter(Collision collision)
    {
        if (exploded) return;

        //if (collision.gameObject.CompareTag("Player"))
        //{
        //    return;
        //}

        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();

        if (rb != null && collision.relativeVelocity.magnitude > velMin)
        {
            if(activate == false)
            {
                Shatter();
            }
            else
            {
                Activate();
            }
        }
    }
    void Shatter()
    {
        PrefabGun.instance.Refund(2);
        Destroy(gameObject);
    }

    void Activate()
    {
        door.Open();
    }
}
