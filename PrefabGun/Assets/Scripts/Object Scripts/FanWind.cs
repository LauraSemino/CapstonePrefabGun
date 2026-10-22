using UnityEngine;

public class FanWind : MonoBehaviour
{
    public float windness = 20f; // How wind the wind is
    public float range = 4.5f;    // How far the wind blows
    public float radius = 1f;    // Width of the wind

    void FixedUpdate()
    {
        Vector3 origin = transform.position - transform.up * radius + transform.up * 0.25f;

        if (Physics.SphereCast(origin, radius, transform.up, out RaycastHit hit, range + radius))
        {
            FanWind wind = hit.transform.gameObject.GetComponentInChildren<FanWind>();
            if (hit.rigidbody != null && (wind == null || !wind.enabled))
                
                hit.rigidbody.AddForce(transform.up * windness, ForceMode.Acceleration);
        }
    }
}
