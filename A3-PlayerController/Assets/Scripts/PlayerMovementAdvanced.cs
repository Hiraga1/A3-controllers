using Cinemachine;
using System.Collections;
using UnityEngine;

public class PlayerMovementAdvanced : MonoBehaviour
{
    [SerializeField] private InputHandler input;
    public InputHandler InputHandler => input;

    public bool IsRegister => input.HasInputActionMap;

    private Rigidbody rb;
    public Vector3 Velocity => rb.velocity;

    private bool isAiming;

    private bool isSprinting;
    public bool IsSprinting => isSprinting;

    private bool isCrouching;
    public bool IsCrouching => IsCrouching;

    [SerializeField]
    private CinemachineVirtualCamera freeLookCam;

    // Cinemachine Virtual Camera (for aiming)
    [SerializeField]
    private CinemachineVirtualCamera aimCamera;

    private CinemachineBasicMultiChannelPerlin channelPerlin;

    [SerializeField]
    private float transitionDuration = 0.5f;

    [SerializeField]
    private GameObject Crosshair;

    private float currentLerpTime;

    [Header("Movement")]
    public float walkSpeed;

    public float sprintSpeed;
    public float slideSpeed;
    public float wallrunSpeed;

    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    private float rotationSpeed;

    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;

    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    private bool readyToJump;
    public bool canDoubleJump;
    private bool readyToLeap;
    public float leapCD;


    [Header("Crouching")]
    public float crouchSpeed;

    public float crouchYScale;
    private float startYScale;

    [Header("Ground Check")]
    public float playerHeight;

    public LayerMask whatIsGround;
    public bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;

    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("References")]
    public Climbing climbingScript;

    public Throwing throwScript;
    public PlayerStatus status;
    public Transform orientation;

    [Header("Falling")]
    public float maxFallingThreshold = 15;

    private float horizontalInput;
    private float verticalInput;

    [Header("Animation")]
    private Animator characterAnimator;

    
    public bool isChaser;

    public MovementState state;

    public enum MovementState
    {
        idle,
        walking,
        sprinting,
        sliding,
        air,
        climbing,
        freeze,
        crouching,
        leaping,
        wallrunning
    }

    public bool sliding;
    public bool climbing;
    public bool freeze;

    public bool sprinting;

    public bool leaping;
    public bool jumping;
    public bool wallrunning;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;
        startYScale = transform.localScale.y;

        input.RegisterOnJumpInput(jump);

        input.RegisterOnCrouchPress(beginCrouch);
        input.RegisterOnCrouchCancel(cancelCrouch);

        input.RegisterOnAimPress(() => isAiming = true);
        input.RegisterOnAimCancel(() => isAiming = false);

        input.RegisterOnSprintPress(() => isSprinting = true);
        input.RegisterOnSpiritCancel(() => isSprinting = false);

        input.SetupCinemachineCameraControl(freeLookCam);
        input.SetupCinemachineCameraControl(aimCamera);
    }

    private void Update()
    {
        bool previousGrounded = grounded;
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);

        if (!previousGrounded && grounded)
        {
            if (rb.velocity.y < -maxFallingThreshold)
            {
                status.Stun();
            }
        }

        if (previousGrounded && !grounded)
        {
            state = MovementState.air;
        }
        else if (grounded)
        {
            state = rb.velocity == Vector3.zero ? MovementState.idle : MovementState.walking;
            if (isSprinting)
            {
                state = MovementState.sprinting;
            }
            else if (isCrouching)
            {
                state = MovementState.crouching;
            }
        }

        stateHandler();

        speedControl();

        if (isAiming)
        {
            desiredMoveSpeed = 2;
        }

        SwitchCamera();
    }

    private void FixedUpdate()
    {
        if (climbingScript.exitingWall) return;
        movePlayer();

        if (rb.velocity.magnitude > desiredMoveSpeed)
            rb.velocity = rb.velocity.normalized * desiredMoveSpeed;
        rb.useGravity = !OnSlope();
    }

    private void movePlayer()
    {
        // calculate movement direction
        var moveDirection = orientation.forward * input.MovementInput.y + orientation.right * input.MovementInput.x;

        // on slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * desiredMoveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        // on ground
        else if (grounded)
            rb.AddForce(moveDirection * desiredMoveSpeed * 10, ForceMode.Force);

        // in air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * desiredMoveSpeed * 10f * airMultiplier, ForceMode.Force);

        // turn gravity off while on slope
        rb.useGravity = !OnSlope();
    }

    private void speedControl()
    {
        lastDesiredMoveSpeed = desiredMoveSpeed;
        switch (state)
        {
            case MovementState.walking:
                desiredMoveSpeed = walkSpeed;
                break;

            case MovementState.sprinting:
                desiredMoveSpeed = sprintSpeed;
                break;

            case MovementState.crouching:
                desiredMoveSpeed = crouchSpeed;
                break;

            case MovementState.wallrunning:
                desiredMoveSpeed = wallrunSpeed;
                break;

            case MovementState.sliding:
                desiredMoveSpeed = slideSpeed;
                break;

            case MovementState.freeze:
                desiredMoveSpeed = 0;
                break;

            default:
                break;
        }
    }

    private void stateHandler()
    {
        // Mode - Wallrunning
        if (wallrunning)
        {
            state = MovementState.wallrunning;
        }
        //Mode - Freeze
        if (freeze)
        {
            state = MovementState.freeze;
            rb.velocity = Vector3.zero;
        }
        // Mode - Sliding
        else if (sliding)
        {
            state = MovementState.sliding;
        }
    }

        

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - desiredMoveSpeed);
        float startValue = desiredMoveSpeed;

        while (time < difference)
        {
            desiredMoveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
                time += Time.deltaTime * speedIncreaseMultiplier;

            yield return null;
        }
    }

    #region Jump

    private void jump()
    {
        exitingSlope = true;

        if (readyToJump && grounded)
        {
            readyToJump = false;

            performJump();

            Invoke(nameof(resetJump), jumpCooldown);
        }
        else if (canDoubleJump)
        {
            performJump();

            canDoubleJump = false;
        }

        void performJump()
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
        }
    }

    private void resetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    #endregion Jump

    #region Crouch

    private void beginCrouch()
    {
        isCrouching = true;
        transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
        rb.AddForce(Vector3.down * 10f, ForceMode.Impulse);
        state = MovementState.crouching;
    }

    private void cancelCrouch()
    {
        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        isCrouching = false;
    }

    #endregion Crouch

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    private void SwitchCamera()
    {
        if (isAiming)
        {
            // Start camera transition to aim camera
            if (freeLookCam.Priority > aimCamera.Priority)
            {
                currentLerpTime += Time.deltaTime;
                if (currentLerpTime > transitionDuration)
                    currentLerpTime = transitionDuration;

                // Lerp the Priority values
                float lerpValue = currentLerpTime / transitionDuration;
                freeLookCam.Priority = Mathf.RoundToInt(Mathf.Lerp(10, 0, lerpValue));
                aimCamera.Priority = Mathf.RoundToInt(Mathf.Lerp(5, 15, lerpValue));
                Crosshair.SetActive(true);
                freeLookCam.gameObject.SetActive(false);
                aimCamera.gameObject.SetActive(true);
            }
        }
        else
        {
            // Start camera transition back to FreeLook camera
            if (aimCamera.Priority > freeLookCam.Priority)
            {
                currentLerpTime += Time.deltaTime;
                if (currentLerpTime > transitionDuration)
                    currentLerpTime = transitionDuration;

                // Lerp the Priority values
                float lerpValue = currentLerpTime / transitionDuration;
                freeLookCam.Priority = Mathf.RoundToInt(Mathf.Lerp(0, 10, lerpValue));
                aimCamera.Priority = Mathf.RoundToInt(Mathf.Lerp(15, 5, lerpValue));
                Crosshair.SetActive(false);
                freeLookCam.gameObject.SetActive(true);
                aimCamera.gameObject.SetActive(false);
                var pov = freeLookCam.GetCinemachineComponent<CinemachinePOV>();
                if (pov != null)
                {
                    pov.m_HorizontalAxis.Value = gameObject.transform.eulerAngles.y;  // Reset Horizontal (yaw)
                    pov.m_VerticalAxis.Value = 0f;    // Reset Vertical (pitch)
                }
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Runner")
        {
           
        }
    }
}