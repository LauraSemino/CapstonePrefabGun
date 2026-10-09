using System.Security.Cryptography;
using UnityEngine;

public class PlacedObjectDeleter : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Debug.Log(other.name);
            PrefabGun pg = other.GetComponentInChildren<PrefabGun>();
            foreach (GameObject savedObject in pg.placedObjects)
            {
                if(savedObject != null)
                {
                    pg.Refund(savedObject.GetComponent<ObjectData>().objData.cost);
                    Destroy(savedObject);
                }
            }
            pg.placedObjects.Clear();
        }
    }
}
