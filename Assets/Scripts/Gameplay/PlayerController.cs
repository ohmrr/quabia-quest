using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Functionality")]
    [SerializeField] private bool jumpEnabled = true;
    [SerializeField] private bool sprintEnabled = true;
    [SerializeField] private bool crouchEnabled = true;
    private bool controlsEnabled = true;

    [Header("Movement Options")]
    [SerializeField] private float walkSpeed = 2.5f;
    [SerializeField] private float sprintSpeed = 4.0f;
    private Vector3 currentMovement = Vector3.zero;
    private CharacterController characterController;

    [Header("Look Options")]
    [SerializeField] private float lookSensitivity = 0.5f;
    [SerializeField] private float lookAngleRange = 85.0f;
    private float verticalRotation = 0.0f;

    [Header("Jump Options")]
    [SerializeField] private float jumpForce = 5.0f;
    [SerializeField] private float gravity = 9.81f;

    [Header("Footstep Audio Options")]
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private float walkStepInterval = 0.6f;
    [SerializeField] private float sprintStepInterval = 0.3f;
    [SerializeField] private float velocityThreshold = 2.0f;
    private int lastPlayedIndex = -1;
    private float nextStepTime;

    [Header("Camera Options")]
    private Camera playerCamera;

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction sprintAction;
    private InputAction jumpAction;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        footstepSource = GetComponentInChildren<AudioSource>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        moveAction = InputSystem.actions.FindAction("Move");
        lookAction = InputSystem.actions.FindAction("Look");
        sprintAction = InputSystem.actions.FindAction("Sprint");
        jumpAction = InputSystem.actions.FindAction("Jump");
    }

    private void Update()
    {
        if (controlsEnabled)
        {
            HandleLook();
            HandleMovement();
            HandleFootstepLoop();
        }
    }

    void HandleLook()
    {
        Vector2 lookValue = lookAction.ReadValue<Vector2>() * lookSensitivity;
        transform.Rotate(0, lookValue.x, 0);

        verticalRotation -= lookValue.y;
        verticalRotation = Mathf.Clamp(verticalRotation, -lookAngleRange, lookAngleRange);

        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    void HandleMovement()
    {
        float speedMultiplier = isSprinting() ? sprintSpeed : walkSpeed;

        Vector2 movementInput = moveAction.ReadValue<Vector2>() * speedMultiplier;

        Vector3 horizontalMovement = new Vector3(movementInput.x, 0, movementInput.y);
        horizontalMovement = transform.rotation * horizontalMovement;

        if (jumpEnabled)
        {
            HandleJump();
        }

        currentMovement.x = horizontalMovement.x;
        currentMovement.z = horizontalMovement.z;

        characterController.Move(currentMovement * Time.deltaTime);
    }

    void HandleFootstepLoop()
    {
        float stepInterval = isSprinting() ? sprintStepInterval : walkStepInterval;

        if (characterController.isGrounded && isWalking() && Time.time > nextStepTime && characterController.velocity.magnitude > velocityThreshold)
        {
            PlayFootstepSounds();
            nextStepTime = Time.time + stepInterval;
        }
    }

    void PlayFootstepSounds()
    {
        int randomIndex = Random.Range(0, footstepSounds.Length - 1);
        if (randomIndex >= lastPlayedIndex) {
            randomIndex++;
        }

        lastPlayedIndex = randomIndex;
        footstepSource.clip = footstepSounds[randomIndex];
        footstepSource.Play();
    }

    void HandleJump()
    {
        if (characterController.isGrounded)
        {
            currentMovement.y = -0.5f;

            if (jumpAction.IsPressed())
            {
                currentMovement.y = jumpForce;
            }
        }
        else
        {
            currentMovement.y -= gravity * Time.deltaTime;
        }
    }

    bool isSprinting()
    {
        return sprintAction.IsPressed() && sprintEnabled;
    }

    bool isWalking()
    {
        return moveAction.ReadValue<Vector2>().magnitude > 0;
    }
}
