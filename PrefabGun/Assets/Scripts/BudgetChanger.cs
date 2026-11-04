using UnityEngine;

public class BudgetChanger : MonoBehaviour
{
    public enum BudgetMode { Set, Add }
    public BudgetMode mode = BudgetMode.Add;
    public int setBudget;
    public bool isCollected = false;
    private void Start()
    {    
        if (isCollected)
        {
            gameObject.SetActive(false);
        }
    }
    private void OnCollect()
    {
        //mark in save file as collected, if collected the "IsCollected" variable will always be true
        isCollected = true;
        gameObject.SetActive(false);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Debug.Log(other.name);
            PrefabGun pg = other.GetComponentInChildren<PrefabGun>();

            if(mode == BudgetMode.Set)
            {
                pg.maxBudget = setBudget;
            }
            else if (mode == BudgetMode.Add)
            {
                pg.maxBudget += setBudget;             
                OnCollect();
            }
            pg.BuildBudgetLines();
            pg.UpdateBudgetUI();
        }
    }
}
