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

    float objProjectionDistance = 0;

    public Transform displayPoint;
    private GameObject displayedObject;

    // Update is called once per frame
    void Update()
    {
        //follows mouse better in update
        if (isPlaceMode && toPlace != null)
        {
            toPlace.transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w);
            toPlace.transform.SetParent(transform, true);
            placeAdjust += placeIncriment;
            toPlace.transform.rotation = Quaternion.Euler(toPlace.transform.rotation.x, toPlace.transform.rotation.y + (placeAdjust), toPlace.transform.rotation.z);
        }
    }
    //scanning and placing
    public void OnLeftClick(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if(!isPlaceMode)
            {
                LayerMask grabObjectsLayer = LayerMask.GetMask("Prefab");
                RaycastHit hit;
                if (Physics.SphereCast(transform.position, 0.25f, transform.forward, out hit, 5f, grabObjectsLayer))
                {
                    if (ScanCheck(hit.collider.gameObject.GetComponent<ObjectData>().objData.id))
                    {
                        GameObject savedObject = Instantiate(hit.collider.gameObject);
                        savedObject.SetActive(false);
                        savedObjects.Add(savedObject);
                        UpdateDisplay();
                    }
                }
            }
            else
            {
                if(toPlace != null)
                {
                    objProjectionDistance = 2;
                    Destroy(toPlace.gameObject);
                }
                isPlaceMode = false;
            }
        }
    }
    public void OnRightClick(InputAction.CallbackContext context)
    {
        if (context.started)
        {        
            toPlace = Instantiate(savedObjects[curObjIndex], new Vector3(transform.position.x, transform.position.y , transform.position.z) + transform.forward * 2, new Quaternion(0, transform.rotation.y, 0, transform.rotation.w));
            toPlace.SetActive(true);
            toPlace.GetComponent<MeshRenderer>().material = canBePlaced;
            toPlace.GetComponent<Collider>().isTrigger = true;
            toPlace.GetComponent<Rigidbody>().isKinematic = true;
            isPlaceMode = true;
           /* RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, 5f))
            {
                
            }*/
        }
        if (context.canceled && toPlace != null)
        {
            isPlaceMode = false;
            Vector3 placePos = toPlace.transform.position;
            Quaternion placeRot = toPlace.transform.rotation;
            Destroy(toPlace.gameObject);
            toPlace = null;
            GameObject placedObject = Instantiate(savedObjects[curObjIndex], placePos, new Quaternion(0, placeRot.y, 0, placeRot.w));
            placedObject.SetActive(true);
            placeAdjust = 0;
            objProjectionDistance = 2;
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
                UpdateDisplay();
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
                UpdateDisplay();
            }
        }
        if(context.canceled)
        {
            placeIncriment = 0;
        }
    }

    public void OnPushPullObject(InputAction.CallbackContext context)
    {
        if(isPlaceMode && toPlace != null)
        {
            //needs distance limiter
            float i = context.ReadValue<float>();
            objProjectionDistance += i;
            if(objProjectionDistance <= 10 && objProjectionDistance >= 2)
            {
                toPlace.transform.position += i * transform.forward;
            }
            else if (objProjectionDistance > 10)
            {
                objProjectionDistance = 10;
            }
            else if (objProjectionDistance < 2)
            {
                objProjectionDistance = 2;
            }
            Debug.Log(objProjectionDistance);
        }
    }

    void UpdateDisplay()
    {
        if (savedObjects.Count == 0) return;

        displayIndex.text = curObjIndex + 1 + "/" + savedObjects.Count;

        if (displayedObject != null)
        {
            Destroy(displayedObject);
        }
        displayedObject = Instantiate(savedObjects[curObjIndex], displayPoint);
        displayedObject.SetActive(true);
        displayedObject.transform.localPosition = Vector3.zero;
        displayedObject.transform.localRotation = Quaternion.identity;

        Vector3 originalScale = displayedObject.transform.localScale;
        displayedObject.transform.localScale = originalScale * 0.5f;

        foreach (Collider col in displayedObject.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        foreach (Rigidbody rb in displayedObject.GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = true;
        }
    }

    bool ScanCheck(int incomingID)
    {
        //checks if the object has already been scanned into the gun
        foreach (GameObject obj in savedObjects)
        {
            if (incomingID == obj.GetComponent<ObjectData>().objData.id)
            {
                return false;
            }
        }
        return true;
    }
}
