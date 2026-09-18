using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] public float walkSpeed;
    [SerializeField] private float jumpStrength;
    [SerializeField] private float lookSensitivity;
    [SerializeField] private float lookXLimit;
    [SerializeField] private float gravity;

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private CharacterController characterController;
    private Rigidbody rb;
    private bool canMove = true;
    public float originalWalkSpeed;

    // for audio
    bool alreadyWalking;
    bool tryJump;
    Vector2 moveInput;
    Vector2 lookInput;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        alreadyWalking = false;
    }


    void Update()
    {
        //movement vectors
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float currentSpeedX = canMove ? (walkSpeed) * moveInput.y : 0;
        float currentSpeedY = canMove ? (walkSpeed) * moveInput.x : 0;
        float movementDirectionY = moveDirection.y;

        //interpret the vectors and multiply for the direction accordingly
        moveDirection = (forward * currentSpeedX) + (right * currentSpeedY);

        //jump handling
        if (tryJump == true && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpStrength; //add to velocity
            tryJump = false;
        }
        else
        {
            moveDirection.y = movementDirectionY;         
        }

        //gravity
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }
        //Fixes instant terminal velocity bug
        else
        {
            currentSpeedX = walkSpeed;
            if (moveDirection.y < 0)
            {
                moveDirection.y = 0;
            }

            walkSpeed = originalWalkSpeed;

        }
        //adds terminal velocity
        if (moveDirection.y < -15)
        {
            moveDirection.y = -15;
        }

        //apply changes
        characterController.Move(moveDirection * Time.deltaTime);

        //mouse movement
        if (canMove)
        {
            rotationX += -lookInput.y * lookSensitivity;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            mainCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, lookInput.x * lookSensitivity, 0);
        }

        // footstep sounds
        if (new Vector3(characterController.velocity.x, 0, characterController.velocity.z).magnitude > 1 && !alreadyWalking && characterController.isGrounded)
        {
            alreadyWalking = true;
        }
        else if (characterController.velocity.x < 1 && alreadyWalking)
        {
            alreadyWalking = false;
        }
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    public void OnJump(InputAction.CallbackContext context)
    {   if (context.performed)
        {
            tryJump = true;
        }
        else if (context.canceled)
        {
            tryJump = false;
        }
    }
    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }
    public void OnCrouch(InputAction.CallbackContext context)
    {

    }

}
