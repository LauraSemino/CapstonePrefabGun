using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PrefabGun : MonoBehaviour
{
    //0 is scan, 1 is create
    public int mode = 0;
    public MeshRenderer colour;
    [SerializeField] Material green;
    [SerializeField] Material red;
    [SerializeField] Material canBePlaced;

    public List<GameObject> savedObjects;
    public int curObjIndex = 0;

    GameObject grabObject;
    GameObject toPlace = null;

    bool tryLeftMouse;
    bool followMouse;
    [SerializeField] TextMeshProUGUI displayIndex;
    // Update is called once per frame
    void Update()
    { 
        if (mode == 0)
        {
            colour.material = green;
        }
        else if (mode == 1)
        {
            colour.material = red;
        }

        if (followMouse)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, 5f))
            {
                toPlace.transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w);
                toPlace.transform.SetParent(transform, true);
            }
        }
    }
    //scanning and placing
    public void OnLeftClick(InputAction.CallbackContext context)
    { 
        if (context.started)
        {
            LayerMask grabObjectsLayer = LayerMask.GetMask("Prefab");
            RaycastHit hit;
            if (mode == 0)
            {
                if (Physics.SphereCast(transform.position, 0.25f, transform.forward, out hit, 5f, grabObjectsLayer))
                {
                    if (hit.collider != null && hit.collider.gameObject.layer != 10 && hit.collider.gameObject.layer != 11)
                    {
                        Debug.Log("raycast hit");
                        savedObjects.Add(hit.collider.gameObject);
                    }
                }
            }
            else if (mode == 1)
            {
                if (Physics.Raycast(transform.position, transform.forward, out hit, 5f))
                {
                    Instantiate(savedObjects[curObjIndex], new Vector3(hit.point.x, hit.point.y + savedObjects[curObjIndex].gameObject.transform.localScale.y / 2, hit.point.z), new Quaternion(0, transform.rotation.y, 0, transform.rotation.w));
                }
            }
        }
    }
    public void OnRightClick(InputAction.CallbackContext context)
    {
        if(mode == 1)
        {
            if (context.started)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.forward, out hit, 5f))
                {
                    toPlace = Instantiate(savedObjects[curObjIndex], new Vector3(hit.point.x, hit.point.y + savedObjects[curObjIndex].gameObject.transform.localScale.y / 2, hit.point.z), new Quaternion(0, transform.rotation.y, 0, transform.rotation.w));
                    toPlace.GetComponent<MeshRenderer>().material = canBePlaced;
                    toPlace.GetComponent<Collider>().isTrigger = true;
                    toPlace.GetComponent<Rigidbody>().isKinematic = true;
                    followMouse = true;
                }
            }
            if (context.canceled && toPlace != null) 
            {
                followMouse = false;
                Vector3 placePos = toPlace.transform.position;
                Destroy(toPlace.gameObject);
                toPlace = null;
                Instantiate(savedObjects[curObjIndex], placePos, new Quaternion(0, transform.rotation.y, 0, transform.rotation.w));
            }
        }
    }

    public void OnSwapMode(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (mode == 0)
            {
                mode = 1;
            }
            else if (mode == 1)
            {
                mode = 0;
            }
        }
    }
    public void OnSwapIndex(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (curObjIndex < savedObjects.Count - 1)
            {
                curObjIndex += 1;
            }
            else
            {
                curObjIndex = 0;
            }
            displayIndex.text = (curObjIndex + 1).ToString();
        }
    }
}
