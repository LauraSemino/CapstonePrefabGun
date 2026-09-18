using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PrefabGun : MonoBehaviour
{
    //0 is scan, 1 is create
    public MeshRenderer colour;
    [SerializeField] Material green;
    [SerializeField] Material red;
    [SerializeField] Material canBePlaced;

    public List<GameObject> savedObjects;
    public int curObjIndex = 0;

    GameObject grabObject;
    GameObject toPlace = null;
    bool isPlaceMode;
    public int placeIncriment;
    int placeAdjust = 0;
    [SerializeField] TextMeshProUGUI displayIndex;
    // Update is called once per frame
    void Update()
    {
        //follows mouse better in update
        if (isPlaceMode && toPlace != null)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, 5f))
            {
                Debug.Log("hit");
                toPlace.transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w);                
                toPlace.transform.SetParent(transform, true);
                placeAdjust += placeIncriment;
                toPlace.transform.rotation = Quaternion.Euler(toPlace.transform.rotation.x, toPlace.transform.rotation.y + (placeAdjust), toPlace.transform.rotation.z);
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
            if (Physics.SphereCast(transform.position, 0.25f, transform.forward, out hit, 5f, grabObjectsLayer))
            {
                if (hit.collider != null && hit.collider.gameObject.layer != 10 && hit.collider.gameObject.layer != 11)
                {
                    Debug.Log("raycast hit");
                    savedObjects.Add(hit.collider.gameObject);
                }
            }
        }
    }
    public void OnRightClick(InputAction.CallbackContext context)
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
                isPlaceMode = true;
            }
        }
        if (context.canceled && toPlace != null)
        {
            isPlaceMode = false;
            Vector3 placePos = toPlace.transform.position;
            Quaternion placeRot = toPlace.transform.rotation;
            Destroy(toPlace.gameObject);
            toPlace = null;
            Instantiate(savedObjects[curObjIndex], placePos, new Quaternion(0, placeRot.y, 0, placeRot.w));
            placeAdjust = 0;
        }
    }
    public void OnPlusIndex(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (isPlaceMode)
            {
                placeIncriment = -1;
            }
            else
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
        if (context.canceled)
        {
            placeIncriment = 0;
        }
    }
    public void OnMinusIndex(InputAction.CallbackContext context)
    { 
        if (context.started)
        {
            if(isPlaceMode)
            {
                placeIncriment = 1;
            }
            else
            {
                if (curObjIndex > 0)
                {
                    curObjIndex -= 1;
                }
                else
                {
                    curObjIndex = 0;
                }
                displayIndex.text = (curObjIndex + 1).ToString();
            }          
        }
        if(context.canceled)
        {
            placeIncriment = 0;
        }
    }
}
