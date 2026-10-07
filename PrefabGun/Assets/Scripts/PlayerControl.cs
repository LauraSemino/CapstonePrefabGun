using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float lookSensitivity;
    [SerializeField] private float lookXLimit;

    [Header("Movement")]
    [SerializeField] public float walkSpeed; //Player's  max speed on the ground
    [SerializeField] public float airSpeed; // Player's max speed in the air
    [SerializeField] public float groundAcceleration = 5f; // How quickly we speed up on the ground
    [SerializeField] public float airAcceleration = 2f; // How quickly we speed up in the air
    [SerializeField] public float friction = 7f; // How quickly we slow down on the ground
    private Vector3 moveDirection = Vector3.zero; // Which way players wants to move
    Vector3 horVelocity; // Just the horizontal velocity
    float maxSpeed; // Speed limit for the player in state
    float acceleration; // Acceleration for the player in state
    private bool canMove = true;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundMask = 0; // Lowkey just to make player ignore themself as ground mask
    [SerializeField] private float groundCheckDistance = 0.3f; // How far below the feet we look for ground
    [SerializeField] private float maxSlopeAngle = 50f; // Steeper surfaces don't count as ground
    private Vector3 groundUp = Vector3.up; // What is direction of the ground we currently on
    [SerializeField] private float groundSkin = 0.05f; // make the rays start a bit above the feet
    [SerializeField] private float footGroundRingRadius = 0.9f; // Ring of rays as a fraction of the collider radius
    [SerializeField] private int footGroundRays = 8; // rayRings
    [SerializeField] private Transform groundCheckOrigin; // Point from which ground check occurs
    [SerializeField] private float groundCheckRadius = 0.4f; // Radius for the ground check
    private Vector3 steepDirection; // Direction of the too steep thingy
    private bool touchingSteep; // Did we touch too steep thing?

    [Header("Jumping")]
    [SerializeField] private float jumpStrength; // How big we be jumping
    private Vector3 spawnPos; // Where we respawn after dying
    public Rigidbody rb;
    public MeshCollider capsule;
    public bool isGrounded; // we on ground?
    private bool wasGrounded; // Were we grounded?
    public float originalWalkSpeed;
    public Vector3 Velocity; // Our velocity we use and adjust
    PrefabGun prefabGun;

    [Header("Make Jump Feel Good")]
    [SerializeField] private float coyoteTime = 0.1f; // Jump in this time after off ledge
    [SerializeField] private float jumpBufferTime = 0.1f; // Buffer jumps for a lil bit
    private float coyoteTimer;
    private float jumpBufferTimer;

    // So stick on ramp
    [SerializeField] private float groundedStickVelocity = -2f; // Make player stick better to ramps by pushing down into them

    Vector2 moveInput;
    Vector2 lookInput;
    private float lookX; // Horizontal look angle
    private float lookY; // Vertical look angle
    private float groundIgnoreTimer;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        spawnPos = transform.position;
        lookX = transform.eulerAngles.y; // Start looking the way the player object is facing
        prefabGun = mainCamera.gameObject.GetComponent<PrefabGun>();
    }

    void Update()
    {
        DoLook();
    }

    void FixedUpdate()
    {
        DoMovement();
        rb.MoveRotation(Quaternion.Euler(0f, lookX, 0f)); // Turn the body to match the horizontal look angle
    }

    public void PushPlayer(Vector3 force)
    {
        rb.linearVelocity += force; // Big velocity change boom
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        // Buffer jump
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

        lookX += lookInput.x * lookSensitivity; // Turn left/right
        lookY = Mathf.Clamp(lookY - lookInput.y * lookSensitivity, -lookXLimit, lookXLimit); // Look up/down

        mainCamera.transform.rotation = Quaternion.Euler(lookY, lookX, 0f);
    }

    void CheckGround()
    {
        wasGrounded = isGrounded;
        isGrounded = false;
        groundUp = Vector3.up;

        Vector3 center = groundCheckOrigin.position + Vector3.up * groundSkin; // Center  foot check, slight lift so rays don't start inside the floor
        float rayLength = groundSkin + groundCheckDistance;
        float bestUp = -1f; // Tracks the flattest surface hit so far

        for (int i = -1; i < footGroundRays; i++)
        {
            Vector3 origin = center;

            if (i >= 0)
            {
                float footAngle = i * Mathf.PI * 2f / footGroundRays; // Spread the ring rays in a circle to detect areas around player
                origin += new Vector3(Mathf.Cos(footAngle), 0f, Mathf.Sin(footAngle)) * groundCheckRadius * footGroundRingRadius; // Push ray out
            }

            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, rayLength, groundMask, QueryTriggerInteraction.Ignore))
            {
                if (Vector3.Angle(hit.normal, Vector3.up) <= maxSlopeAngle && hit.normal.y > bestUp) // Make sure not walking up something too steep so we can have like ramps but some slight slopes don't allow the player to climb right up
                {
                    bestUp = hit.normal.y;
                    groundUp = hit.normal;
                    isGrounded = true;
                }
            }
        }
    }

    void DoMovement()
    {
        Velocity = rb.linearVelocity; // Start up from the Rigidbody's current velocity

        CheckGround();

        // With new grounded code we need to disable for a lil bit so we can actually jump (no trigger ground check state)
        if (groundIgnoreTimer > 0f)
        {
            groundIgnoreTimer -= Time.fixedDeltaTime;
            isGrounded = false;
        }

        // Coyote timer, counts down once we leave the ground
        if (isGrounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.fixedDeltaTime;

        // Jump buffer timer
        if (jumpBufferTimer > 0f)
            jumpBufferTimer -= Time.fixedDeltaTime;

        // Find the direction that we are trying to go to
        Quaternion facing = transform.rotation;
        moveDirection = facing * Vector3.right * moveInput.x;
        moveDirection += facing * Vector3.forward * moveInput.y;

        if (moveDirection.sqrMagnitude > 0.001f)
            moveDirection.Normalize(); // MAKE SURE THAT DIAGONAL IS NOT FASTER

        // Specific for if a jump happens when a buffered press and coyote time overlap
        bool jumping = jumpBufferTimer > 0f && coyoteTimer > 0f;
        if (jumping)
        {
            jumpBufferTimer = 0f; // Use up the press and the coyote time so we can't double jump
            coyoteTimer = 0f;
            groundIgnoreTimer = 0.15f;
        }

        if (isGrounded)
        {
            // Slide down the slope in direction and make speeds same
            Vector3 slopeDirection = moveDirection - Vector3.Dot(moveDirection, groundUp) * groundUp;
            if (slopeDirection.sqrMagnitude > 0.001f)
                slopeDirection.Normalize();

            // Work with the velocity along the surface
            Vector3 groundVelocity = Velocity - Vector3.Dot(Velocity, groundUp) * groundUp;

            Accelerate(ref groundVelocity, slopeDirection, walkSpeed, groundAcceleration);
            ApplyFriction(ref groundVelocity); // Friction only applies while we are on the ground

            if (groundVelocity.magnitude > walkSpeed)
                groundVelocity = groundVelocity.normalized * walkSpeed;

            Velocity = groundVelocity;
        }
        else
        {
            // Move horizontal in air cuz gravity is vert
            horVelocity = new Vector3(Velocity.x, 0f, Velocity.z);

            Accelerate(ref horVelocity, moveDirection, airSpeed, airAcceleration);

            if (horVelocity.magnitude > airSpeed)
                horVelocity = horVelocity.normalized * airSpeed;

            Velocity.x = horVelocity.x;
            Velocity.z = horVelocity.z;
        }

        // Nonono can't climb too sleep slope
        if (touchingSteep)
        {
            Vector3 flatDir = new Vector3(steepDirection.x, 0f, steepDirection.z);
            if (flatDir.sqrMagnitude > 0.001f)
            {
                flatDir.Normalize();
                float into = Velocity.x * flatDir.x + Velocity.z * flatDir.z;
                if (into < 0f)
                {
                    Velocity.x -= flatDir.x * into;
                    Velocity.z -= flatDir.z * into;
                }
            }
        }
        touchingSteep = false;

        // The speed needed to reach the jump strength height
        if (jumping)
            Velocity.y = Mathf.Sqrt(jumpStrength * -2f * Physics.gravity.y);
        else if (isGrounded)
            Velocity += groundUp * groundedStickVelocity;

        rb.linearVelocity = Velocity;

        // Gravity only in the air cuz it would just push us down into the floor a bit and then makes it weird to jump? so it works better if we just have it in air
        rb.useGravity = !isGrounded || jumping;
    }

    private void Accelerate(ref Vector3 velocity, Vector3 direction, float maxSpeed, float acceleration)
    {
        if (direction.sqrMagnitude < 0.001f)
            return; // No input means there is nothing to accelerate

        float speedInDirection = Vector3.Dot(velocity, direction); // How fast we're already going the direction we going in
        float speedLeft = maxSpeed - speedInDirection;

        if (speedLeft <= 0f)
            return; // Already at max speed in this direction

        float accelerationThisFrame = acceleration * maxSpeed * Time.fixedDeltaTime;

        // Do NOT overshoot max speed
        if (accelerationThisFrame > speedLeft)
            accelerationThisFrame = speedLeft;

        velocity += direction * accelerationThisFrame;
    }

    private void ApplyFriction(ref Vector3 velocity)
    {
        float currentSpeed = velocity.magnitude;

        // Snap to a stop when nearly still so that we can avoid sliding forever
        if (currentSpeed < 0.01f)
        {
            velocity = Vector3.zero;
            return;
        }

        float speedLost = currentSpeed * friction * Time.fixedDeltaTime; // Faster means more friction
        float newSpeed = Mathf.Max(currentSpeed - speedLost, 0f);

        velocity *= newSpeed / currentSpeed; // Keep direction but scale it all down
    }

    void ResetPlayer() // Reset player on death
    {
        Debug.Log(spawnPos);
        rb.position = spawnPos;
        rb.linearVelocity = Vector3.zero;
        Velocity = Vector3.zero;
    }

    private void OnTriggerEnter(Collider other) // Hazard collision
    {
        if (other.tag == "Hazard")
        {
            ResetPlayer();
        }
    }
}


