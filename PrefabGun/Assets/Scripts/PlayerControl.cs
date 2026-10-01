using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float lookSensitivity;
    [SerializeField] private float lookXLimit;
    private float cameraRotation;

    [Header("Movement")]
    [SerializeField] public float walkSpeed;
    [SerializeField] public float airSpeed;
    [SerializeField] public float groundAcceleration = 5f;
    [SerializeField] public float airAcceleration = 2f;
    [SerializeField] public float friction = 7f;
    private Vector3 moveDirection = Vector3.zero;
    Vector3 flatVelocity;
    float maxSpeed;
    float acceleration;
    [SerializeField] private float speedMult = 1f;
    [SerializeField, Range(0f, 1f)] private float landSpeedKeep = 0.95f;
    private bool canMove = true;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundMask = 0;
    [SerializeField] private float groundCheckDistance = 0.15f;
    [SerializeField] private float maxSlopeAngle = 50f;
    private Vector3 groundNormal = Vector3.up;
    [SerializeField] private float groundRayLength = 1.1f;
    [SerializeField] private float groundSkin = 0.05f;
    private float jumpGroundIgnoreTimer;

    [Header("Jumping")]
    [SerializeField] private float jumpStrength;
    [SerializeField] private float gravity;
    private Vector3 spawnPos;
    private Rigidbody rb;
    private CapsuleCollider capsule;
    public bool isGrounded;
    private bool wasGrounded;
    public float originalWalkSpeed;
    public bool JumpFrame;
    public Vector3 Velocity;
    PrefabGun prefabGun;

    [Header("Make Jump Feel Good")]
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    private float coyoteTimer;
    private float jumpBufferTimer;

    // So stick on ramp
    [SerializeField] private float groundedStickVelocity = -2f;

    Vector2 moveInput;
    Vector2 lookInput;
    private float yaw;
    private float pitch;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponentInChildren<CapsuleCollider>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        spawnPos = transform.position;
        yaw = transform.eulerAngles.y;
        prefabGun = mainCamera.gameObject.GetComponent<PrefabGun>();
    }

    void Update()
    {
        DoLook();
    }

    void FixedUpdate()
    {
        DoMovement();
        rb.MoveRotation(Quaternion.Euler(0f, yaw, 0f));
    }

    public void PushPlayer(Vector3 force)
    {
        rb.linearVelocity += force;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
            jumpBufferTimer = jumpBufferTime;
    }
    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }
    public void OnCrouch(InputAction.CallbackContext context)
    {

    }

    void DoLook()
    {
        float mouseX = lookInput.x * lookSensitivity;
        float mouseY = lookInput.y * lookSensitivity;

        yaw += lookInput.x * lookSensitivity;
        pitch = Mathf.Clamp(pitch - lookInput.y * lookSensitivity, -lookXLimit, lookXLimit);

        cameraRotation -= mouseY;
        cameraRotation = Mathf.Clamp(cameraRotation, -lookXLimit, lookXLimit);

        mainCamera.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void CheckGround()
    {
        wasGrounded = isGrounded;
        isGrounded = false;
        groundNormal = Vector3.up;

        if (jumpGroundIgnoreTimer > 0f)
        {
            jumpGroundIgnoreTimer -= Time.fixedDeltaTime;
            return;
        }

        float radius = capsule.radius * Mathf.Max(Mathf.Abs(capsule.transform.lossyScale.x), Mathf.Abs(capsule.transform.lossyScale.z)) * 0.95f;

        Vector3 origin = capsule.bounds.center;
        origin.y = capsule.bounds.min.y + radius + groundSkin;

        if (Physics.SphereCast(origin, radius, Vector3.down, out RaycastHit hit, groundSkin + groundCheckDistance, groundMask, QueryTriggerInteraction.Ignore))
        {
            if (Vector3.Angle(hit.normal, Vector3.up) <= maxSlopeAngle)
            {
                isGrounded = true;
                groundNormal = hit.normal;
            }
        }
    }

    void DoMovement()
    {
        Velocity = rb.linearVelocity;

        CheckGround();
        bool justLanded = isGrounded && !wasGrounded;

        if (isGrounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.fixedDeltaTime; ;

        if (jumpBufferTimer > 0f)
            jumpBufferTimer -= Time.fixedDeltaTime; ;

        Quaternion facing = transform.rotation;
        moveDirection = facing * Vector3.right * moveInput.x;
        moveDirection += facing * Vector3.forward * moveInput.y;

        if (moveDirection.sqrMagnitude > 0.001f)
            moveDirection.Normalize();

        flatVelocity = new Vector3(Velocity.x, 0f, Velocity.z);

        maxSpeed = walkSpeed;
        acceleration = groundAcceleration;

        if (!isGrounded)
        {
            maxSpeed = airSpeed;
            acceleration = airAcceleration;
        }

        if (justLanded)
            flatVelocity *= landSpeedKeep;

        Accelerate(ref flatVelocity, moveDirection, maxSpeed, acceleration);

        if (isGrounded)
            ApplyFriction(ref flatVelocity);

        float speedCap = maxSpeed * speedMult;

        if (flatVelocity.magnitude > speedCap)
            flatVelocity = flatVelocity.normalized * speedCap;
        bool jumping = false;
        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            jumping = true;
            JumpFrame = true;
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
        }

        if (jumping)
        {
            Velocity.x = flatVelocity.x;
            Velocity.z = flatVelocity.z;
            Velocity.y = Mathf.Sqrt(jumpStrength * -2f * gravity);
        }
        else if (isGrounded)
        {
            Vector3 slopeVelocity = Vector3.ProjectOnPlane(flatVelocity, groundNormal);
            if (slopeVelocity.sqrMagnitude > 0.0001f)
                slopeVelocity = slopeVelocity.normalized * flatVelocity.magnitude;

            Velocity = slopeVelocity;
            Velocity.y += groundedStickVelocity;
        }
        else
        {
            Velocity.x = flatVelocity.x;
            Velocity.z = flatVelocity.z;
            Velocity.y += gravity * Time.fixedDeltaTime;
        }

        rb.linearVelocity = Velocity;
    }

    public void LateUpdate()
    {
        JumpFrame = false;
    }

    private void Accelerate(ref Vector3 velocity, Vector3 direction, float maxSpeed, float acceleration)
    {
        if (direction.sqrMagnitude < 0.001f)
            return;

        float speedInDirection = Vector3.Dot(velocity, direction);
        float speedLeft = maxSpeed - speedInDirection;

        if (speedLeft <= 0f)
            return;

        float accelerationThisFrame = acceleration * maxSpeed * Time.fixedDeltaTime;

        if (accelerationThisFrame > speedLeft)
            accelerationThisFrame = speedLeft;

        velocity += direction * accelerationThisFrame;
    }

    private void ApplyFriction(ref Vector3 velocity)
    {
        float currentSpeed = velocity.magnitude;

        if (currentSpeed < 0.01f)
        {
            velocity = Vector3.zero;
            return;
        }

        float speedLost = currentSpeed * friction * Time.fixedDeltaTime;
        float newSpeed = Mathf.Max(currentSpeed - speedLost, 0f);

        velocity *= newSpeed / currentSpeed;
    }

    void ResetPlayer()
    {
        Debug.Log(spawnPos);
        rb.position = spawnPos;
        rb.linearVelocity = Vector3.zero;
        Velocity = Vector3.zero;
        Physics.SyncTransforms();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Hazard")
        {
            ResetPlayer();
        }
    }
}
