using Unity.VisualScripting;
using UnityEngine;

public class GivePrefabGun : MonoBehaviour
{
    [SerializeField] GameObject prefabGunModel;
    [SerializeField] float rotationSpeed;
    float rotation;
    [SerializeField] bool collected;
    void FixedUpdate()
    {
        if(!collected)
        {
            rotation += rotationSpeed;
            prefabGunModel.transform.rotation = Quaternion.Euler(0f, rotation, 0f);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            prefabGunModel.SetActive(false);
            other.GetComponentInChildren<PrefabGun>().hasGun = true;
            collected = true;
        }
    }
}
