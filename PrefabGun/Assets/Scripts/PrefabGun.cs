
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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
    public bool hasGun;

    [Header("Object Storage")]
    public List<GenericObject> savedObjects;
    public HashSet<int> savedIDs;
    public int curObjIndex = 0;
    public List<GameObject> placedObjects;

    [Header("Object Placement")]
    public float minDistancePlace = 2.5f;
    public float maxDistancePlace = 10f;
    Vector3 rotationInput;
    Vector3 rotationOffset;
    public float rotationSpeed = 100f;
    float objProjectionDistance = 0;
    GameObject toPlace = null;
    GameObject placeIndicator = null;
    bool isPlaceMode;
    [SerializeField] LayerMask placementBlockMask;
    [SerializeField] float placementSurfaceOffset = 0.05f;
    public float placeCooldown = 2f;

    [Header("Display")]
    [SerializeField] TextMeshProUGUI displayIndex;
    [SerializeField] GameObject cost1;
    [SerializeField] GameObject cost2;

    public Transform displayPoint;
    private GameObject displayedObject;
    [SerializeField] GameObject prefabGunModel;
    [SerializeField] Material highlightMat;
    [SerializeField] GameObject placeVisual;
    [SerializeField] GameObject arrows;
 
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
    readonly Collider[] overlaps = new Collider[16];
    [SerializeField] float maxPushDistance = 1.5f;
    Renderer[] previewRenderers;
    bool placementValid = true;

    [Header("Player Ignore")]
    [SerializeField] LayerMask playerMask;
    readonly RaycastHit[] rayHits = new RaycastHit[16];
    private ObjectOutline currentOutlined;
    [SerializeField] LayerMask noBuildZone;
    [SerializeField] private PlayerControl player;

    [Header("Budget Feedback")]
    [SerializeField] float shakeDuration = 0.4f;
    Vector3 budgetBaseScale = Vector3.one;
    Quaternion budgetBaseRotation = Quaternion.identity;
    Vector2 budgetBasePos;
    Coroutine budgetShakeRoutine;
    private bool shaking = false;
    [SerializeField] RectTransform budgetPreview;
    Image previewImage;
    [SerializeField] RectTransform budgBar;
    [SerializeField] Image linePrefab;
    [SerializeField] float lineThickness = 2f;
    [SerializeField] Color lineColor;
    readonly List<RectTransform> budgetLines = new List<RectTransform>();
    private float timer;

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
        budgetBasePos = ((RectTransform)budgetBar.transform).anchoredPosition;
        budgetBaseScale = ((RectTransform)budgetBar.transform).localScale;
        budgetBaseRotation = ((RectTransform)budgetBar.transform).rotation;
        if (budgetPreview != null) previewImage = budgetPreview.GetComponent<Image>();
        instance = this;
        UpdateDisplay();
        player = GetComponentInParent<PlayerControl>();
        //eventually this is gonna need to be saved per file
        hasGun = false;
        BuildBudgetLines();
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
        //prolly unoptimal cause its setting it every frame
        prefabGunModel.SetActive(hasGun);

        //display control arrows
        if(savedObjects.Count > 1)
        {
            arrows.SetActive(true);
        }
        else { arrows.SetActive(false); }

        if (!DeleteMenuManager.IsOpen)
        {
            //applies shaders //heavy on performance
            ObjectOutline hitOutline = null;

            if (hasGun && !isPlaceMode)
            {
                LayerMask grabObjectsLayer = LayerMask.GetMask("Prefab");

                if (Physics.SphereCast(transform.position, 0.25f, transform.forward, out RaycastHit hit, 5f, grabObjectsLayer))
                {
                    hitOutline = hit.collider.GetComponentInParent<ObjectOutline>();
                }
            }

            // Turn off the old outline when aiming elsewhere
            if (currentOutlined != hitOutline)
            {
                if (currentOutlined != null)
                    currentOutlined.SetOutline(false);

                currentOutlined = hitOutline;

                // Turn on the new outline
                if (currentOutlined != null)
                    currentOutlined.SetOutline(true);
            }
        }
        UpdateCostPreview();
        timer += Time.deltaTime;
    }

    void ClearAimOutline()
    {
        if (currentOutlined != null)
            currentOutlined.SetOutline(false);
        currentOutlined = null;
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
        if (!hasGun) { return; }
        if (DeleteMenuManager.IsOpen) return;
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
        if(!hasGun) { return; }
        if (context.started && DeleteMenuManager.IsOpen) return;
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
        if (timer < placeCooldown) return;
        if (savedObjects.Count == 0) return;
        if (isPlaceMode) return;
        GenericObject OG = savedObjects[curObjIndex];
        if (curBudget + OG.cost > maxBudget)
        {
            if (!shaking)
               ShowNotEnoughBudget();
            return;
        }
        // Set stats to default and show preview
        isPlaceMode = true;
        rotationInput = Vector3.zero;
        rotationOffset = Vector3.zero;
        objProjectionDistance = targetProjectionDistance = minDistancePlace;
        snapTo = true;
        toPlace = Instantiate(OG.prefab);
        Preview(toPlace);

        placeIndicator = Instantiate(placeVisual, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0), toPlace.transform);
        placeIndicator.transform.localScale = new Vector3(1 / toPlace.transform.localScale.x, placeVisual.transform.localScale.y, 1 / toPlace.transform.localScale.z);
        placeIndicator.transform.localPosition = new Vector3(0f, -placeIndicator.transform.localScale.y * 0.5f, 0f);
        UpdatePlacement();
    }


    // Place down object
    public void ConfirmPlace()
    {
        if (!isPlaceMode || toPlace == null) return;
        if (!placementValid)
        {
            CancelPlace();
            return;
        }

        Vector3 placePos = targetPosition;
        Quaternion placeRot = targetRotation;
        Destroy(toPlace);
        toPlace = null;
        GenericObject OG = savedObjects[curObjIndex];

        // Place down object prefab
        if (TrySpend(OG.cost))
        {
            GameObject placedDownObject = Instantiate(OG.prefab, placePos, placeRot);
            placedObjects.Add(placedDownObject);
            placedDownObject.SetActive(true);

            ObjectData data = placedDownObject.GetComponent<ObjectData>();
            data.createdByPlayer = true;
        }
        timer = 0;
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

        int rayMask = placementBlockMask & ~playerMask;

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hitInfo,
            objProjectionDistance, rayMask, QueryTriggerInteraction.Ignore))
        {
            wantedPosition = hitInfo.point + hitInfo.normal * placementSurfaceOffset;
        }

        // Figure out the target using position we wanna reach
        toPlace.transform.SetPositionAndRotation(wantedPosition, wantedRotation);
        Physics.SyncTransforms();

        bool success = PushOutOfObstacles();
        bool wentTooFaar = (toPlace.transform.position - wantedPosition).sqrMagnitude
                            > maxPushDistance * maxPushDistance;
        bool valid = success && !wentTooFaar & !TouchingNoBuildZone();

        if (!valid)
        {
            toPlace.transform.position = wantedPosition;
        }

        SetPlacementValid(valid);

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

    void SetPlacementValid(bool valid)
    {
        if (valid == placementValid) return;
        placementValid = valid;

        Material mat = valid ? canBePlaced : red;
        if (mat == null || previewRenderers == null) return;

        foreach (Renderer r in previewRenderers)
        {
            if (r == null) continue;

            Material[] materials = r.sharedMaterials;

            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = mat;
            }

            r.sharedMaterials = materials;
        }
    }


    bool PushOutOfObstacles()
    {
        // Get all colliders
        Collider[] myCols = toPlace.GetComponentsInChildren<Collider>();

        // Try multiple times to ensure success
        for (int passAttemptTry = 0; passAttemptTry < 6; passAttemptTry++)
        {
            bool moved = false;

            foreach (Collider mine in myCols)
            {
                // Get bounds and make an overlap box for collision
                Bounds b = mine.bounds;
                int count = Physics.OverlapBoxNonAlloc(b.center, b.extents, overlaps, Quaternion.identity, placementBlockMask, QueryTriggerInteraction.Ignore);


                for (int i = 0; i < count; i++)
                {
                    // Find overlaps
                    Collider other = overlaps[i];
                    if (other.transform.IsChildOf(toPlace.transform)) continue;

                    // Don't push on player
                    if (((1 << other.gameObject.layer) & playerMask) != 0)
                    {
                        if (Physics.ComputePenetration(mine, mine.transform.position, mine.transform.rotation,
                            other, other.transform.position, other.transform.rotation, out _, out _))
                        {
                            return false;
                        }
                        continue;
                    }


                    // Find wall penetration for avoiding collision with walls
                    if (Physics.ComputePenetration(mine, mine.transform.position, mine.transform.rotation, other, other.transform.position, other.transform.rotation, out Vector3 dir, out float dist))
                    {
                        if (dir.y < 0f) dir.y = 0f;
                        if (dir.sqrMagnitude < 0.0001f) return false;
                        dir.Normalize();
                        toPlace.transform.position += dir * (dist + placementSurfaceOffset);
                        Physics.SyncTransforms();
                        moved = true;
                    }
                }
            }

            if (!moved) return true;
        }

        return false;
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
        targetProjectionDistance = Mathf.Clamp(targetProjectionDistance + i, minDistancePlace, maxDistancePlace);
    }

    // remove objects that player placed
    public void OnRemoveObject(InputAction.CallbackContext context)
    {
        if (!hasGun || !player.isGrounded) { return; }
        if (context.performed)
        {
            if (isPlaceMode)
            {
                CancelPlace();
                return;
            }
            ClearAimOutline();
            DeleteMenuManager.instance.Toggle();
        }
    }


    // Rotate the object positive
    public void OnPlusIndex(InputAction.CallbackContext context)
    {
        if (!hasGun) { return; }
        if (DeleteMenuManager.IsOpen) return;
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
        if (!hasGun) { return; }
        if (DeleteMenuManager.IsOpen) return;
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
        // Remove the outline material from the gun display
        foreach (Renderer renderer in display.GetComponentsInChildren<Renderer>())
        {
            Material[] materials = renderer.sharedMaterials;

            if (materials.Length > 1)
            {
                Material[] normalMaterials = new Material[materials.Length - 1];

                for (int i = 0; i < normalMaterials.Length; i++)
                {
                    normalMaterials[i] = materials[i];
                }

                renderer.sharedMaterials = normalMaterials;
            }
        }

        // Disable the prefab's colliders
        foreach (Collider col in display.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        // Disable physics
        foreach (Rigidbody body in display.GetComponentsInChildren<Rigidbody>())
        {
            body.isKinematic = true;
            body.detectCollisions = false;
        }

        // Disable prefab behaviours
        foreach (MonoBehaviour behaviour in display.GetComponentsInChildren<MonoBehaviour>())
        {
            behaviour.enabled = false;
        }

        // disable particles
        foreach (ParticleSystem ps in display.GetComponentsInChildren<ParticleSystem>())
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.GetComponent<ParticleSystemRenderer>().enabled = false;
        }

        foreach (Transform trs in display.GetComponentsInChildren<Transform>())
        {
            trs.gameObject.layer = 8;
        }
    }
    public void BuildBudgetLines()
    {
        if (budgBar == null || linePrefab == null || maxBudget <= 0f) return;

        int needed = Mathf.Max(0, Mathf.CeilToInt(maxBudget) - 1);

        while (budgetLines.Count < needed)
        {
            Image img = Instantiate(linePrefab, budgBar);
            img.color = lineColor;
            budgetLines.Add(img.rectTransform);
        }

        for (int i = 0; i < budgetLines.Count; i++)
        {
            bool active = i < needed;
            budgetLines[i].gameObject.SetActive(active);
            if (!active) continue;

            float t = (i + 1) / maxBudget;

            RectTransform rt = budgetLines[i];
            rt.anchorMin = new Vector2(0.05f, t);
            rt.anchorMax = new Vector2(0.95f, t);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(0f, lineThickness);
        }
    }


    void UpdateCostPreview()
    {
        bool placing = isPlaceMode && toPlace != null && savedObjects.Count > 0;
        float flash = (Mathf.Sin(Time.time * 5f) + 1f) * 0.5f;
        if (budgetPreview == null) return;
        budgetPreview.gameObject.SetActive(placing);
        if (!placing || maxBudget <= 0f) return;

        float from = Mathf.Clamp01(curBudget / maxBudget);
        float to = Mathf.Clamp01((curBudget + savedObjects[curObjIndex].cost) / maxBudget);

        budgetPreview.anchorMin = new Vector2(0f, from);
        budgetPreview.anchorMax = new Vector2(1f, to);
        budgetPreview.offsetMin = new Vector2(2f, 0f);
        budgetPreview.offsetMax = new Vector2(-2f, 0f);

        if (previewImage != null)
        {
            Color c = Color.black;
            c.a = Mathf.Lerp(0.25f, 1f, flash);
            previewImage.color = c;
        }
    }

    public void UpdateBudgetUI()
    {
        if (budgetBar == null || maxBudget <= 0f) return;
        float percent = Mathf.Clamp01(curBudget / maxBudget);
        budgetBar.size = percent;
        Image img = budgetBar.GetComponentInChildren<Image>();
        if (percent > 0.5f && percent < 0.75) img.color = Color.yellow;
        else if (percent < 0.5) img.color = Color.green;
        else if (percent > 0.75f) img.color = Color.red;
    }

    void ShowNotEnoughBudget()
    {
        if (budgetShakeRoutine != null)
            StopCoroutine(budgetShakeRoutine);
        budgetShakeRoutine = StartCoroutine(BudgetShake());
    }

    IEnumerator BudgetShake()
    {
        shaking = true;
        RectTransform rt = (RectTransform)budgetBar.transform;
        Image[] imgs = budgetBar.GetComponentsInChildren<Image>(); // Get bar image
        Image img = imgs[1];

        UpdateBudgetUI();
        img.color = Color.Lerp(img.color, Color.red, 0.7f); // Change color

        float timed = 0f;
        while (timed < shakeDuration)
        {
            timed += Time.deltaTime;
            float going = timed / shakeDuration;
            float fade = 1f - going;

            Vector2 offset = Vector2.zero;
            float angle = 0f;
            angle = Mathf.Sin(going * Mathf.PI * 2f * 3f) * 7f * Mathf.Exp(-4f * going); // Change angle for shake
            offset.y = Mathf.Abs(Mathf.Sin(going * Mathf.PI * 2f * 3f)) * 0.4f * fade; // Change offset for shake
            rt.anchoredPosition = budgetBasePos + offset; // Set pos and orot to offset and angle
            rt.localRotation = budgetBaseRotation * Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }

        // Put everything back to the norm
        rt.anchoredPosition = budgetBasePos;
        rt.localRotation = budgetBaseRotation;
        rt.localScale = budgetBaseScale;
        img.color = Color.Lerp(img.color, Color.black, 1f);
        UpdateBudgetUI();
        shaking = false;
        budgetShakeRoutine = null;
    }

    bool TouchingNoBuildZone()
    {
        foreach (Collider mine in toPlace.GetComponentsInChildren<Collider>())
        {
            Bounds b = mine.bounds;
            if (Physics.CheckBox(b.center, b.extents, Quaternion.identity, noBuildZone, QueryTriggerInteraction.Collide))
                return true;
        }
        return false;
    }

    // Shows the object at place location before placement
    public void Preview(GameObject prev)
    {
        foreach (Collider col in prev.GetComponentsInChildren<Collider>())
        {
            if (col.isTrigger)
                col.enabled = false;

            col.isTrigger = true;
        }

        foreach (Rigidbody body in prev.GetComponentsInChildren<Rigidbody>())
        {
            body.isKinematic = true;
            body.detectCollisions = false;
        }

        foreach (MonoBehaviour behaviour in prev.GetComponentsInChildren<MonoBehaviour>())
        {
            behaviour.enabled = false;
        }

        foreach (ParticleSystem ps in prev.GetComponentsInChildren<ParticleSystem>())
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.GetComponent<ParticleSystemRenderer>().enabled = false;
        }

        previewRenderers = prev.GetComponentsInChildren<Renderer>();
        placementValid = true;

        // Remove the outline and apply the green material
        foreach (Renderer renderer in previewRenderers)
        {
            Material[] materials = renderer.sharedMaterials;

            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = canBePlaced;
            }

            renderer.sharedMaterials = materials;
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
        if(OG.cost == 1)
        {
           cost1.SetActive(true);
           cost2.SetActive(false);
        }
        else
        {
            cost2.SetActive(true);
            cost1.SetActive(false);
        }
        displayedObject.SetActive(true);
        displayedObject.transform.localPosition = Vector3.zero;
        displayedObject.transform.localRotation = Quaternion.identity;
        displayedObject.transform.localScale *= 0.5f;

        Display(displayedObject);
    }
}



