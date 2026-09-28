using UnityEngine;

public class RefundFloor : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Hit");
        if(other.gameObject.layer == 3)
        {
            Debug.Log("Layered");
            ObjectData od = other.GetComponent<ObjectData>();
            PrefabGun.instance.Refund(od.objData.cost);
            Destroy(other.gameObject);
        }
    }
}
