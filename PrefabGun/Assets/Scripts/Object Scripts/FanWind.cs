using UnityEngine;

public class FanWind : MonoBehaviour
{
    public float windness = 20f; // How wind the wind is
    public float range = 4.5f;    // How far the wind blows
    public float radius = 1f;    // Width of the wind
    public bool on = true;

    public LayerMask blowable;

    void FixedUpdate()
    {
        Vector3 origin = transform.position - transform.up * radius;
        if (on)
            if (Physics.SphereCast(origin, radius, transform.up, out RaycastHit hit, range + radius, blowable , QueryTriggerInteraction.Ignore))
            {
             FanWind wind = hit.transform.gameObject.GetComponentInChildren<FanWind>();
              if (hit.rigidbody != null && (wind == null || !wind.enabled))
                 
                 hit.rigidbody.AddForce(transform.up * windness, ForceMode.Acceleration);
           }
       }

    public void FanState()
    {
        ParticleSystem ps = GetComponentInChildren<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.GetComponent<ParticleSystemRenderer>().enabled = false;
        Animator am = GetComponentInChildren<Animator>();
        am.enabled = false;
        on = false;
    }
}
