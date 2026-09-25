using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PrefabGun : MonoBehaviour
{
    //0 is scan, 1 is create, 2 is delete

    [Header("Gun Visual Components")]
    public MeshRenderer colour;
    [SerializeField] Material green;
    [SerializeField] Material red;
    [SerializeField] Material canBePlaced;

    [Header("Object Storage")]
    public List<GenericObject> savedObjects;
    public HashSet<int> savedIDs;
    public int curObjIndex = 0;

    [Header("Object Placement")]
    public float minDistancePlace = 2.5f;
    public float maxDistancePlace = 10f;
    Vector3 rotationInput;
    Vector3 rotationOffset;
    public float rotationSpeed = 100f;
    float objProjectionDistance = 0;
    GameObject toPlace = null;
    bool isPlaceMode;

    [Header("Display")]
    [SerializeField] TextMeshProUGUI displayIndex;
    public Transform displayPoint;
    private GameObject displayedObject;

    [Header("Budget")]
    [SerializeField] float maxBudget;
    [SerializeField] Scrollbar budgetBar;
    public float curBudget;


    // Game default settings
    private void Awake()
    {
        savedIDs = new HashSet<int>();
        savedIDs.Clear();
        foreach (GenericObject obj in savedObjects)
        {
            if (obj != null)
                savedIDs.Add(obj.id);
        }
        SetGunMode(false);
        UpdateDisplay();

    }

    // Update is called once per frame
    void Update()
    {
        //follows mouse better in update
        if (isPlaceMode && toPlace != null)
        {
            if (rotationInput != Vector3.zero)
            {
                rotationOffset += rotationInput * rotationSpeed * Time.deltaTime;
            }
            
            UpdatePlacement();
        }
    }

    public void UpdateBudgetUI()
    {
        if (budgetBar != null || maxBudget <= 0f)
        {
            budgetBar.size = Mathf.Clamp01(curBudget / maxBudget);
        }
    }

    bool TrySpend(float cost)
    {
        if (curBudget + cost > maxBudget) return false;

        curBudget += cost;
        UpdateBudgetUI();
        return true;

    }

    void Refund(float cost)
    {
        curBudget = Mathf.Max(0f, curBudget - cost);
        UpdateBudgetUI();
    }

    //scanning
    public void OnLeftClick(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (isPlaceMode)
            {
                //change movement mode
                return;
            }
            else
            {
                ScanObject();
            }
        }
    }

    //placing
    public void OnRightClick(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            StartPlace();
        }
        if (context.canceled)
        {
            ConfirmPlace();
        }
    }


    // Object scanning
    public void ScanObject()
    {
        // Cast and find hit
        LayerMask grabObjectsLayer = LayerMask.GetMask("Prefab");
        RaycastHit hit;
        if (!Physics.SphereCast(transform.position, 0.25f, transform.forward, out hit, 5f, grabObjectsLayer))
        {
            return;
        }

        // Set object data based
        ObjectData objData = hit.collider.gameObject.GetComponent<ObjectData>();
        int objID = objData.objData.id;
        if (savedIDs.Contains(objData.objData.id))
        {
            return;
        }
        savedIDs.Add(objData.objData.id);
        savedObjects.Add(objData.objData);
        UpdateDisplay();
    }

    

    // Start the object placement process
    public void StartPlace()
    {
        if (savedObjects.Count == 0)
            return;
        if (isPlaceMode)
            return;

        // Set stats to default and show preview
        isPlaceMode = true;
        rotationInput = Vector3.zero;
        rotationOffset = Vector3.zero;
        objProjectionDistance = minDistancePlace;
        GenericObject OG = savedObjects[curObjIndex];
        toPlace = Instantiate(OG.prefab);
        toPlace.transform.SetParent(transform, true);
        SetGunMode(true);
        Preview(toPlace);
        UpdatePlacement();
    }

    // Place down object
    public void ConfirmPlace()
    {
        if (!isPlaceMode || toPlace == null) return;

        Vector3 placePos = toPlace.transform.position;
        Quaternion placeRot = toPlace.transform.rotation;
        Destroy(toPlace);
        toPlace = null;
        GenericObject OG = savedObjects[curObjIndex];

        // Place down object prefab
        if (TrySpend(OG.cost))
        {
            GameObject placedDownObject = Instantiate(OG.prefab, placePos, placeRot);
            placedDownObject.SetActive(true);

            ObjectData data = placedDownObject.GetComponent<ObjectData>();
            data.createdByPlayer = true;
        }

        ExitPlace();
    }

    // Stope the placing down process
    public void ExitPlace()
    {
        isPlaceMode = false;
        rotationInput = Vector3.zero;
        rotationOffset = Vector3.zero;
        objProjectionDistance = minDistancePlace;
        SetGunMode(false);
    }

    // Stop the placement
    public void CancelPlace()
    {
        if (toPlace != null)
        {
            Destroy(toPlace);
            toPlace = null;
        }
        ExitPlace();
    }

    // Update the placement of the object to be placed
    public void UpdatePlacement()
    {
        if (toPlace == null)
            return;

        Vector3 position = transform.position + transform.forward * objProjectionDistance;
        Quaternion rotation = Quaternion.Euler(0f, transform.eulerAngles.y + rotationOffset.y, 0f);
        toPlace.transform.SetPositionAndRotation(position, rotation);

    }

    // Goes to the next object
    public void NextObject()
    {
        if (savedObjects.Count == 0)
            return;
        curObjIndex++;
        if (curObjIndex >= savedObjects.Count)
            curObjIndex = 0;
        UpdateDisplay();
    }

    // Goes to the previous object
    public void PreviousObject()
    {
        if (savedObjects.Count == 0)
            return;
        curObjIndex--;
        if (curObjIndex < 0)
            curObjIndex = savedObjects.Count - 1;
        UpdateDisplay();
    }

    // Move objects pre-place with mouse wheel
    public void OnPushPullObject(InputAction.CallbackContext context)
    {
        if (!isPlaceMode && toPlace == null)
            return;

        float i = context.ReadValue<float>();
        objProjectionDistance += i;
        objProjectionDistance = Mathf.Clamp(objProjectionDistance, minDistancePlace, maxDistancePlace);
        UpdatePlacement();
        Debug.Log(objProjectionDistance);

    }

    // remove objects that player placed
    public void OnRemoveObject(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (isPlaceMode)
            {
                CancelPlace();
                return;
            }
            LayerMask grabObjectsLayer = LayerMask.GetMask("Prefab");
            RaycastHit hit;
            if (Physics.SphereCast(transform.position, 0.25f, transform.forward, out hit, 5f, grabObjectsLayer))
            {
                if (hit.collider.gameObject.GetComponent<ObjectData>().createdByPlayer == true)
                {
                    Refund(hit.collider.gameObject.GetComponent<ObjectData>().objData.cost);
                    Destroy(hit.collider.gameObject);
                }
            }
        }
    }

    // Rotate the object positive
    public void OnPlusIndex(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (isPlaceMode)
            {
                rotationInput.y = -1;
                return;
            }
            NextObject();
        }
        if (context.canceled && isPlaceMode)
        {
            rotationInput.y = 0f;

        }
    }

    // Rotate the object negative
    public void OnMinusIndex(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (isPlaceMode)
            {
                rotationInput.y = 1;
                return;
            }
            PreviousObject();
        }
        if (context.canceled && isPlaceMode)
        {
            rotationInput.y = 0f;

        }
    }

    // Changes the guns mode
    public void SetGunMode(bool placing)
    {
        if (colour == null)
            return;

        colour.sharedMaterial = placing ? red : green;
    }

    // Displays the object in gun
    public void Display(GameObject display)
    {
        // Removes the functionality of the prefabs
        foreach (Collider col in display.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        foreach (Rigidbody body in display.GetComponentsInChildren<Rigidbody>())
        {
            body.isKinematic = true;
            body.detectCollisions = false;
        }

        foreach (MonoBehaviour behaviour in
                 display.GetComponentsInChildren<MonoBehaviour>())
        {
            behaviour.enabled = false;
        }
    }

    // Shows the object at place location before placement
    public void Preview(GameObject prev)
    {
        // Removes the functionality of the prefabs
        foreach (Collider col in prev.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        foreach (Rigidbody body in prev.GetComponentsInChildren<Rigidbody>())
        {
            body.isKinematic = true;
            body.detectCollisions = false;
        }

        foreach (MonoBehaviour behaviour in
                 prev.GetComponentsInChildren<MonoBehaviour>())
        {
            behaviour.enabled = false;
        }

        if (canBePlaced != null)
        {
            foreach (Renderer renderer in
                     prev.GetComponentsInChildren<Renderer>())
            {
                renderer.sharedMaterial = canBePlaced;
            }
        }
    }

    // Update the prefab gun's UI
    void UpdateDisplay()
    {

        displayIndex.text = savedObjects.Count == 0 ? "0/0" : curObjIndex + 1 + "/" + savedObjects.Count;

        if (displayedObject != null)
        {
            Destroy(displayedObject);
            displayedObject = null;
        }

        if (savedObjects.Count == 0) return;

        GenericObject OG = savedObjects[curObjIndex];

        displayedObject = Instantiate(OG.prefab, displayPoint);

        displayedObject.SetActive(true);
        displayedObject.transform.localPosition = Vector3.zero;
        displayedObject.transform.localRotation = Quaternion.identity;
        displayedObject.transform.localScale *= 0.5f;

        Display(displayedObject);
    }
}

