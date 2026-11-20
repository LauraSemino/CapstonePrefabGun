using UnityEngine;

public class PlacedObjectDeleter : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            PrefabGun pg = other.GetComponentInChildren<PrefabGun>();
            foreach (GameObject savedObject in pg.placedObjects)
            {
                if (savedObject != null)
                {
                    pg.Refund(savedObject.GetComponent<ObjectData>().objData.cost);
                    Destroy(savedObject);
                }
            }
            pg.placedObjects.Clear();
            //adjust rotation as well so the player is facking the right way
            //other.GetComponent<PlayerControl>().spawnPos = transform.rotation;
            PlayerControl player = other.GetComponent<PlayerControl>();
            player.spawnPos = transform.position;

            if(GetComponent<SaveID>() != null)
               SaveManager.Instance.SetLastDeleter(GetComponent<SaveID>().id);
        }
    }
}
