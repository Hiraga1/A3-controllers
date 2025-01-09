
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementAdvanced : MonoBehaviour
{
    private InputActionAsset inputAsset;
    private InputActionMap player;
    private InputAction move;
    //private Third_Person_View ControllerControls;

    //InputHandler inputManager;
    Vector3 moveDirection;
    Rigidbody rb;
    public Transform cameraObject;



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

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.C;

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
        swinging,
        crouching,
        grappling,
        jumping,
        leaping,
        wallrunning
    }

    public bool sliding;
    public bool climbing;
    public bool freeze;
    public bool swinging;
    public bool sprinting;
    public bool activeGrapple;
    public bool leaping;
    public bool jumping;
    public bool wallrunning;

    public bool inAction;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;

        startYScale = transform.localScale.y;
    }

    private void Awake()
    {
        //inputManager = GetComponent<InputHandler>();
         
    }
    //private void HandleMovement()
    //{
    //    moveDirection = cameraObject.forward * inputManager.verticalInput;
    //    moveDirection = moveDirection + cameraObject.right * inputManager.horizontalInput;
    //    moveDirection.Normalize(); ;
    //    moveDirection.y = 0;

    //    if (grounded)
    //    {
    //        rb.AddForce(moveDirection * moveSpeed * 10f, ForceMode.Force);


    //    }
    //    else if (!grounded)
    //        rb.AddForce(moveDirection * moveSpeed * 10f * airMultiplier, ForceMode.Force);
    //    if (OnSlope() && !exitingSlope)
    //    {
    //        rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);


    //        if (rb.velocity.y > 0)
    //            rb.AddForce(Vector3.down * 80f, ForceMode.Force);
    //    }

    //    // on ground
    //    else if (grounded)
    //    {
    //        rb.AddForce(moveDirection * moveSpeed * 10f, ForceMode.Force);


    //    }

    //    // in air
    //    else if (!grounded)
    //        rb.AddForce(moveDirection * moveSpeed * 10f * airMultiplier, ForceMode.Force);

    //    // turn gravity off while on slope
    //}

    //public void HandleAllMovement()
    //{
    //    HandleMovement();
    //}


    //private void OnEnable()
    //{


    //    //ControllerControls.Enable();
    //    player = inputAsset.FindActionMap("Player");
    //}



    //private void OnDisable()
    //{


    //    //ControllerControls.Disable();
    //    player = inputAsset.FindActionMap("Player");
    //}
    private void Update()
    {
        Debug.Log(moveDirection);
        bool previousGrounded = grounded;
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();
        if (!previousGrounded && grounded)
        {
            if (rb.velocity.y < -maxFallingThreshold)
            {
                status.Stun();
            }
        }

        if (sprinting && grounded)
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }
        // crouching Gamepad
        if (Gamepad.current.buttonWest.wasPressedThisFrame)
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 10f, ForceMode.Impulse);
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }
        else if (Gamepad.current.buttonWest.wasReleasedThisFrame)
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }
        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 10f, ForceMode.Impulse);
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }
        else if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }

        // handle drag
        if (grounded && !activeGrapple)
        {
            canDoubleJump = true;
            rb.drag = groundDrag;
        }

        else
            rb.drag = 0;
        if (Input.GetKey(KeyCode.Mouse0))
        {
            moveSpeed = 2;
        }
        if (Gamepad.current.rightTrigger.isPressed)
        {
            moveSpeed = 2;
        }


    }

    private void SprintPress()
    {
        sprinting = true;
    }
    private void SprintRelease()
    {
        sprinting = false;
    }


    private void FixedUpdate()
    {
        if (climbingScript.exitingWall) return;

        rb.useGravity = !OnSlope();
    }

    public void MyInput()
    {
        

        // when to jump Gamepad
        if (Gamepad.current.buttonEast.wasPressedThisFrame)
        {
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

        // when to jump Keyboard
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            exitingSlope = true;

            if (readyToJump && grounded)
            {
                readyToJump = false;

                rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
                jumping = true;
                Invoke(nameof(ResetJump), jumpCooldown);
            }
            else if (canDoubleJump)
            {
                rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
                jumping = true;
                canDoubleJump = false;
            }
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

   

    private void SpeedControl()
    {
        if (activeGrapple)
        {
            return;
        }
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
    private bool enableMovementOnNextTouch;

    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {
        activeGrapple = true;

        velocityToSet = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);

        Invoke(nameof(SetVelocity), 0.1f);
    }

    private Vector3 velocityToSet;

    private void SetVelocity()
    {
        enableMovementOnNextTouch = true;

        rb.velocity = velocityToSet;
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

    private void OnCollisionEnter(Collision collision)
    {
        if (enableMovementOnNextTouch)
        {
            enableMovementOnNextTouch = false;
            ResetRestrictions();

            //GetComponent<Grappling>().StopGrapple();

        }
    }
    public void ResetRestrictions()
    {
        activeGrapple = false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }
    public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity)
            + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

        return velocityXZ + velocityY;
    }
    //public IEnumerator PerformVaulting(string animName, MatchTargetParameters matchTargetParameters = null, Quaternion targetRotation = new Quaternion(), bool shouldRotate = false, bool mirrored = false)
    //{
    //    inAction = true;

    //    yield return null;

    //    var animState = characterAnimator.GetNextAnimatorStateInfo(0);

    //    float rotateStartTime = (matchTargetParameters != null) ? matchTargetParameters.matchStartTime : 0;

    //    float time = 0.0f;

    //    while (time <= animState.length)
    //    {
    //        time += Time.deltaTime;
    //        float normalizedTime = time / animState.length;

    //        if (shouldRotate && normalizedTime > rotateStartTime)
    //        {
    //            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    //        }
    //        if (matchTargetParameters != null)
    //            MatchTarget(matchTargetParameters);

    //        yield return null;
    //    }
    //    inAction = false;
    //}

    //private void MatchTarget(MatchTargetParameters matchtargetParams)
    //{
    //    if (characterAnimator.isMatchingTarget || characterAnimator.IsInTransition(0)) return;

    //    characterAnimator.MatchTarget(matchtargetParams.matchPos, transform.rotation, matchtargetParams.matchBodyPart, new MatchTargetWeightMask(matchtargetParams.matchPosWeight, 0.0f), matchtargetParams.matchStartTime, matchtargetParams.matchTargetTime);
    //}


}
