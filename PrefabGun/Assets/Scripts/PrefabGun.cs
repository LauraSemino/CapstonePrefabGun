
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PrefabGun : MonoBehaviour
{
    //0 is scan, 1 is create, 2 is delete

    public static PrefabGun instance;

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
    [SerializeField] LayerMask placementBlockMask;
    [SerializeField] float placementSurfaceOffset = 0.05f;

    [Header("Display")]
    [SerializeField] TextMeshProUGUI displayIndex;
    public Transform displayPoint;
    private GameObject displayedObject;

    [Header("Budget")]
    [SerializeField] float maxBudget;
    [SerializeField] Scrollbar budgetBar;
    public float curBudget;

    [Header("Placement Smoothing")]
    public float positionSmoothTime = 0.06f;
    public float rotationSmoothSpeed = 15f;
    public float distanceSmoothSpeed = 12f;
    Vector3 smoothVelocity;
    Vector3 smoothedPosition;
    Quaternion smoothedRotation;
    Vector3 targetPosition;
    Quaternion targetRotation;
    float targetProjectionDistance;
    bool snapTo;



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
        instance = this;
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
        if (budgetBar == null || maxBudget <= 0f) return;
        float percent = Mathf.Clamp01(curBudget / maxBudget);
        Debug.Log(percent);
        budgetBar.size = percent;
        Image img = budgetBar.GetComponent<Image>();
        if (percent > 0.6f && percent < 0.8) img.color = Color.yellow;
        else if (percent < 0.4 && percent < 0.6) img.color = Color.green;
        else if (percent > 0.8f) img.color = Color.red;
    }


    bool TrySpend(float cost)
    {
        if (curBudget + cost > maxBudget) return false;

        curBudget += cost;
        UpdateBudgetUI();
        return true;

    }

    public void Refund(float cost)
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
        objProjectionDistance = targetProjectionDistance = minDistancePlace;
        snapTo = true;
        GenericObject OG = savedObjects[curObjIndex];
        toPlace = Instantiate(OG.prefab);
        Preview(toPlace);
        UpdatePlacement();
    }

    // Place down object
    public void ConfirmPlace()
    {
        if (!isPlaceMode || toPlace == null) return;

        Vector3 placePos = targetPosition;
        Quaternion placeRot = targetRotation;
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
        objProjectionDistance = targetProjectionDistance = minDistancePlace;
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

        // LERPINGGGGGGG lerp the distance
        objProjectionDistance = Mathf.Lerp(objProjectionDistance, targetProjectionDistance, 1f - Mathf.Exp(-distanceSmoothSpeed * Time.deltaTime));

        Vector3 wantedPosition = transform.position + transform.forward * objProjectionDistance;
        Quaternion wantedRotation = Quaternion.Euler(0f, transform.eulerAngles.y + rotationOffset.y, 0f);

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hitInfo,
            objProjectionDistance, placementBlockMask, QueryTriggerInteraction.Ignore))
        {
            wantedPosition = hitInfo.point + hitInfo.normal * placementSurfaceOffset;
        }

        // Figure out the target using position we wanna reach
        toPlace.transform.SetPositionAndRotation(wantedPosition, wantedRotation);
        Physics.SyncTransforms();
        PushOutOfFloor();
        PushOutOfWalls();

        targetPosition = toPlace.transform.position;
        targetRotation = wantedRotation;

        // Smooth toward where we wanna go
        if (snapTo)
        {
            snapTo = false;
            smoothVelocity = Vector3.zero;
            smoothedPosition = targetPosition;
            smoothedRotation = targetRotation;
        }
        else
        {
            smoothedPosition = Vector3.SmoothDamp(smoothedPosition, targetPosition, ref smoothVelocity, positionSmoothTime);
            smoothedRotation = Quaternion.Slerp(smoothedRotation, targetRotation, 1f - Mathf.Exp(-rotationSmoothSpeed * Time.deltaTime));
        }

        toPlace.transform.SetPositionAndRotation(smoothedPosition, smoothedRotation);
    }

    // Big box that covers the object
    Bounds GetObjectBox()
    {
        Physics.SyncTransforms();

        Renderer[] allRenderers = toPlace.GetComponentsInChildren<Renderer>();

        // Start with the first renderer's box, then grow it to fit the rest
        Bounds box = allRenderers[0].bounds;

        foreach (Renderer oneRenderer in allRenderers)
        {
            Collider c = oneRenderer.GetComponent<Collider>();
            if (c != null && c.isTrigger)
                continue;

            box.Encapsulate(oneRenderer.bounds);
        }

        return box;
    }

    // No more clipping into the floor stuff
    void PushOutOfFloor()
    {
        Bounds box = GetObjectBox();

        // Start a ray above the object and shoot it straight down
        Vector3 rayStart = new Vector3(box.center.x, box.max.y + 0.5f, box.center.z);
        float rayLength = box.size.y + maxDistancePlace;

        RaycastHit floorHit;
        bool foundFloor = Physics.Raycast(rayStart, Vector3.down, out floorHit, rayLength, placementBlockMask, QueryTriggerInteraction.Ignore);

        if (foundFloor == true)
        {
            float bottomOfObject = box.min.y;
            float floorHeight = floorHit.point.y;

            if (bottomOfObject < floorHeight)
            {
                float amountToLift = floorHeight - bottomOfObject + placementSurfaceOffset;
                toPlace.transform.position += Vector3.up * amountToLift;
            }
        }
    }

    // Push the object away from walls on all four sides
    void PushOutOfWalls()
    {
        Bounds box = GetObjectBox();

        // How far inside a wall are we on each side?
        float insideRightWall = HowDeepInWall(Vector3.right, box.extents.x, box);
        float insideLeftWall = HowDeepInWall(Vector3.left, box.extents.x, box);
        float insideFrontWall = HowDeepInWall(Vector3.forward, box.extents.z, box);
        float insideBackWall = HowDeepInWall(Vector3.back, box.extents.z, box);

        // Move the opposite way of each wall
        Vector3 pushAmount = Vector3.zero;
        pushAmount = pushAmount - Vector3.right * insideRightWall;
        pushAmount = pushAmount - Vector3.left * insideLeftWall;
        pushAmount = pushAmount - Vector3.forward * insideFrontWall;
        pushAmount = pushAmount - Vector3.back * insideBackWall;

        toPlace.transform.position += pushAmount;
    }

    // Looks in one direction for a wall. Returns how far into the wall we are
    float HowDeepInWall(Vector3 direction, float distanceToEdge, Bounds box)
    {
        float deepestAmount = 0f;

        // The three places the rays start from
        Vector3 bottomPoint = new Vector3(box.center.x, box.min.y + 0.2f, box.center.z);
        Vector3 middlePoint = new Vector3(box.center.x, box.center.y, box.center.z);
        Vector3 topPoint = new Vector3(box.center.x, box.max.y - 0.2f, box.center.z);

        RaycastHit hit;

        // Bottom ray
        if (Physics.Raycast(bottomPoint, direction, out hit, distanceToEdge, placementBlockMask, QueryTriggerInteraction.Ignore))
        {
            float amount = distanceToEdge - hit.distance + placementSurfaceOffset;
            if (amount > deepestAmount)
            {
                deepestAmount = amount;
            }
        }

        // Middle ray
        if (Physics.Raycast(middlePoint, direction, out hit, distanceToEdge, placementBlockMask, QueryTriggerInteraction.Ignore))
        {
            float amount = distanceToEdge - hit.distance + placementSurfaceOffset;
            if (amount > deepestAmount)
            {
                deepestAmount = amount;
            }
        }

        // Top ray
        if (Physics.Raycast(topPoint, direction, out hit, distanceToEdge, placementBlockMask, QueryTriggerInteraction.Ignore))
        {
            float amount = distanceToEdge - hit.distance + placementSurfaceOffset;
            if (amount > deepestAmount)
            {
                deepestAmount = amount;
            }
        }

        return deepestAmount;
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
        targetProjectionDistance = Mathf.Clamp(targetProjectionDistance + i, minDistancePlace, maxDistancePlace);
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


