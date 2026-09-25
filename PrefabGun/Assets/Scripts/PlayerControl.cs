using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

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
    Vector3 movementThisFrame;
    float maxSpeed;
    float acceleration;
    [SerializeField] private float speedMult = 1f;
    [SerializeField, Range(0f, 1f)] private float landSpeedKeep = 0.95f;

    [Header("Jumping")]
    [SerializeField] private float jumpStrength;
    [SerializeField] private float gravity;
    private Vector3 spawnPos;
    private CharacterController characterController;
    private bool isGrounded;
    private bool wasGrounded;
    private Rigidbody rb;
    private bool canMove = true;
    public float originalWalkSpeed;
    public bool JumpFrame;
    public Vector3 Velocity;
    private bool jumpPressed;
    PrefabGun prefabGun;

    [Header("Make Jump Feel Good")]
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    private float coyoteTimer;
    private float jumpBufferTimer;

    // So no stuck on ceiling
    [SerializeField] private float ceilingBounce = 0f;

    // So stick on ramp
    [SerializeField] private float groundedStickVelocity = -2f;

    Vector2 moveInput;
    Vector2 lookInput;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        spawnPos = transform.position;
        prefabGun = mainCamera.gameObject.GetComponent<PrefabGun>();
    }

    void Update()
    {
        DoLook();
        DoMovement();
    }

    public void Explode(Vector3 force)
    {
        Velocity += force;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            jumpPressed = true;
            jumpBufferTimer = jumpBufferTime;
        }
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

        transform.Rotate(Vector3.up * mouseX);

        cameraRotation -= mouseY;
        cameraRotation = Mathf.Clamp(cameraRotation, -lookXLimit, lookXLimit);

        Quaternion cameraTurn = Quaternion.Euler(cameraRotation, 0f, 0f);
        mainCamera.transform.localRotation = cameraTurn;
    }

    void DoMovement()
    {
        wasGrounded = isGrounded;
        isGrounded = characterController.isGrounded;
        bool justLanded = isGrounded && !wasGrounded;

        if (isGrounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;

        if (jumpBufferTimer > 0f)
            jumpBufferTimer -= Time.deltaTime;

        if (isGrounded && Velocity.y < 0f)
            Velocity.y = groundedStickVelocity;

        moveDirection = transform.right * moveInput.x;
        moveDirection += transform.forward * moveInput.y;

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

        Velocity.x = flatVelocity.x;
        Velocity.z = flatVelocity.z;

        bool canJumpNow = jumpBufferTimer > 0f && coyoteTimer > 0f;

        if (canJumpNow)
        {
            float jumpVelocity = Mathf.Sqrt(jumpStrength * -2f * gravity);
            Velocity.y = jumpVelocity;
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
            JumpFrame = true;
        }

        jumpPressed = false;

        Velocity.y += gravity * Time.deltaTime;

        movementThisFrame = Velocity * Time.deltaTime;
        characterController.Move(movementThisFrame);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.normal.y < -0.5f && Velocity.y > 0f)
        {
            Velocity.y = ceilingBounce;
        }
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

        float accelerationThisFrame = acceleration * maxSpeed * Time.deltaTime;

        if (accelerationThisFrame > speedLeft)
            accelerationThisFrame = speedLeft;

        Vector3 extraSpeed = direction * accelerationThisFrame;
        velocity += extraSpeed;
    }

    private void ApplyFriction(ref Vector3 velocity)
    {
        float currentSpeed = velocity.magnitude;

        if (currentSpeed < 0.01f)
        {
            velocity = Vector3.zero;
            return;
        }

        float speedLost = currentSpeed * friction * Time.deltaTime;
        float newSpeed = currentSpeed - speedLost;

        if (newSpeed < 0f)
            newSpeed = 0f;

        float speedRatio = newSpeed / currentSpeed;

        velocity *= speedRatio;
    }

    void ResetPlayer()
    {
        Debug.Log(spawnPos);
        gameObject.GetComponent<CharacterController>().enabled = false;
        transform.position = spawnPos;
        gameObject.GetComponent<CharacterController>().enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Hazard")
        {
            ResetPlayer();
        }
    }
}
