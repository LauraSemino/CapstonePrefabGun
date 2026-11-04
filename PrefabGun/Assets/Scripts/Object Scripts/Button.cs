using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PressButton : MonoBehaviour
{
    public LayerMask pressLayers = ~0;
    public float connectRange = 8f;
    public LineRenderer line;
    public int frequency;
    public Transform pressurePlate;
    public float pressDepth;
    public float pressSpeed;

    Vector3 pressPos;
    Vector3 unpressPos;

    List<Door> nearbyDoors = new List<Door>();
    List<Door> heldDoors = new List<Door>();
    HashSet<Collider> pressers = new HashSet<Collider>();
    bool pressed = false;

    void Awake()
    {
        line.positionCount = 0;

        unpressPos = pressurePlate.localPosition;
        pressPos = unpressPos - pressurePlate.up * pressDepth;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;
        if (other.transform.IsChildOf(transform)) return;
        if ((pressLayers.value & (1 << other.gameObject.layer)) == 0) return;

        pressers.Add(other);
    }

    void OnTriggerExit(Collider other)
    {
        pressers.Remove(other);
    }

    void Update()
    {
        pressers.RemoveWhere(c => c == null || !c.enabled || !c.gameObject.activeInHierarchy);

        bool nowPressed = pressers.Count > 0;

        Vector3 targetPos = pressed ? pressPos : unpressPos;

        pressurePlate.localPosition = Vector3.MoveTowards(pressurePlate.localPosition, targetPos, Time.deltaTime * pressSpeed);

        if (nowPressed == pressed) return;
        pressed = nowPressed;

        if (pressed)
        {
            FindDoors();

            foreach (Door door in nearbyDoors)
            {
                if (door.frequency == frequency)
                {
                    door.pressCount++;
                    heldDoors.Add(door);
                }
            }
        }
        else
        {
            ReleaseDoors();
        }
    }



    int FindDoors()
    {
        nearbyDoors.Clear();
        foreach (Door door in Door.All)
        {
            if (door.frequency == frequency)
            {
                if (Vector3.Distance(transform.position + Vector3.up * 0.1f, door.transform.position) <= connectRange)
                    nearbyDoors.Add(door);
            }
        }
        return nearbyDoors.Count;
    }

    public int RefreshConnections()
    {
        int count = FindDoors();

        line.positionCount = count * 2;
        for (int i = 0; i < count; i++)
        {
            line.SetPosition(i * 2, transform.position + Vector3.up * 0.1f);
            line.SetPosition(i * 2 + 1, nearbyDoors[i].transform.position);
        }

        return count;
    }

    void ReleaseDoors()
    {
        foreach (Door door in heldDoors)
        {
            if (door != null) door.pressCount--;
        }
        heldDoors.Clear();
    }

    void OnDisable()
    {
        ReleaseDoors();
        pressers.Clear();
        pressed = false;
    }
}

