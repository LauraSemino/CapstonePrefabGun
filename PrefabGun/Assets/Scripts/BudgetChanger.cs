using UnityEngine;

public class BudgetChanger : MonoBehaviour
{
    public int setBudget;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Debug.Log(other.name);
            PrefabGun pg = other.GetComponentInChildren<PrefabGun>();

            pg.maxBudget = setBudget;
            pg.BuildBudgetLines();
            pg.UpdateBudgetUI();
        }
    }
}
