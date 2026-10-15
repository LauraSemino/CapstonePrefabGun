using UnityEngine;
using System.Collections.Generic;

public class LevelEventSpawn : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public bool oneUse;
    public List<GameObject> toSpawn;
    public List<Transform> spawnPos;
    private void Start()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            for (int i = 0; i < toSpawn.Count; i++)
            {
                Instantiate(toSpawn[i], spawnPos[i]);
            }
            if (oneUse) { gameObject.SetActive(false); }
        }
    }
}
