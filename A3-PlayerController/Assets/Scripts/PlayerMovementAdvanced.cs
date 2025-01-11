
using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class PlayerMovementAdvanced : MonoBehaviour
{
    private Vector2 movementInput;
    
    //private Third_Person_View ControllerControls;

    private InputHandler input;
    Vector3 moveDirection;
    Rigidbody rb;
    
    private bool isAiming;
    public bool isSprinting;
    private bool isCrouching;

    [SerializeField]
    private CinemachineVirtualCamera freeLookCam;
    // Cinemachine Virtual Camera (for aiming)
    [SerializeField]
    private CinemachineVirtualCamera aimCamera;
    CinemachineBasicMultiChannelPerlin channelPerlin;
    [SerializeField]
    private float transitionDuration = 0.5f;
    [SerializeField]
    private GameObject Crosshair;

    private float currentLerpTime;



    [Header("Movement")]
    private float moveSpeed;
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
    bool readyToJump;
    public bool canDoubleJump;
    bool readyToLeap;
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


    float horizontalInput;
    float verticalInput;

    [Header("Animation")]
    private Animator characterAnimator;




    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        sliding,
        air,
        climbing,
        freeze,
        crouching,       
        jumping,
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
        input = GetComponent<InputHandler>();
        input.RegisterOnJumpInput(Jump);

        input.RegisterOnCrouchPress(() => isCrouching = true);
        input.RegisterOnCrouchCancel(() => isCrouching = false);

        input.RegisterOnAimPress(() => isAiming = true);
        input.RegisterOnAimCancel(() => isAiming = false);

        input.RegisterOnSprintPress(() => isSprinting = true);
        input.RegisterOnSprintCancel(() => isSprinting = false);

        input.SetupCinemachineCameraControl(freeLookCam);
        input.SetupCinemachineCameraControl(aimCamera);
    }
    
    private void Update()
    {
        Debug.Log(state);
        bool previousGrounded = grounded;
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);

        
        SpeedControl();
        StateHandler();
        if (!previousGrounded && grounded)
        {
            if (rb.velocity.y < -maxFallingThreshold)
            {
                status.Stun();
            }
        }

        
        if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
            if (isSprinting)
            {
                state  = MovementState.sprinting;
            }
            else if (isCrouching)
            {
                state = MovementState.crouching;
            }
        }
        
        
        if (isAiming)
        {
            moveSpeed = 2;
        }
    }
    private void FixedUpdate()
    {
        if (climbingScript.exitingWall) return;

        rb.useGravity = !OnSlope();
    }
    public void Jump()
    {
        

        // when to jump Gamepad
        
            exitingSlope = true;

            if (readyToJump && grounded)
            {
                readyToJump = false;

                rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);

                Invoke(nameof(ResetJump), jumpCooldown);
            }
            else if (canDoubleJump)
            {
                rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);

                canDoubleJump = false;
            }
                                                                                    

    }

    private void StateHandler()
    {
        // Mode - Wallrunning
        if (wallrunning)
        {
            state = MovementState.wallrunning;
            desiredMoveSpeed = wallrunSpeed;
        }
        //Mode - Freeze
        if (freeze)
        {
            state = MovementState.freeze;
            moveSpeed = 0;
            rb.velocity = Vector3.zero;
        }
        // Mode - Sliding
        else if (sliding)
        {
            state = MovementState.sliding;

            if (OnSlope() && rb.velocity.y < 0.1f)
                desiredMoveSpeed = slideSpeed;

            else
                desiredMoveSpeed = sprintSpeed;
        }


        // check if desiredMoveSpeed has changed drastically
        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

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

        moveSpeed = desiredMoveSpeed;
    }

    private void MovePlayer()
    {
        // calculate movement direction
        moveDirection = orientation.forward * movementInput.y + orientation.right * movementInput.x;

        // on slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        // on ground
        else if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // in air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        // turn gravity off while on slope
        rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        
        // limiting speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }

        // limiting speed on ground or in air
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // limit velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }
    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }
    
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
}

   

   
        
        
       



    








   
    

   
     



    


    
