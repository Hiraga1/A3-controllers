using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class Sliding : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    private CharacterController controller;
    public Transform playerObj;
    private Rigidbody rb;
    private PlayerMovement pm;

    [Header("Sliding")]
    public float maxSlideTime;

    public float slideForce;
    private float slideTimer;

    public float slideYScale;
    private float startYScale;

    private Vector3 playerVelocity;

    [SerializeField]
    private float gravityValue = -9.81f;

    //[Header("Input")]
    //public KeyCode slideKey = KeyCode.LeftControl;

    //private float horizontalInput;
    //private float verticalInput;

    private InputHandler input;

    private void Awake()
    {
        input = GetComponent<InputHandler>();
        input.RegisterOnSlidePress(StartSlide);
        input.RegisterOnSlideCancel(StopSlide);
    }
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
        controller = GetComponent<CharacterController>();
        startYScale = playerObj.localScale.y;
    }

    private void Update()
    {
        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
        //horizontalInput = Input.GetAxisRaw("Horizontal");
        //verticalInput = Input.GetAxisRaw("Vertical");

        //if (Input.GetKeyDown(slideKey) && (horizontalInput != 0 || verticalInput != 0))
        //{
        //    StartSlide();
        //}

        //if (Input.GetKeyUp(slideKey) && pm.sliding)
        //{
        //    StopSlide();
        //}
        //if (Gamepad.current.leftShoulder.wasPressedThisFrame)
        //{
        //    StartSlide();
        //}
        //if (Gamepad.current.leftShoulder.wasReleasedThisFrame)
        //{
        //    StopSlide();
        //}
    }

    private void FixedUpdate()
    {
        
            SlidingMovement();
    }

    private void StartSlide()
    {
        

        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
        rb.AddForce(Vector3.down * 100f, ForceMode.Impulse);

        slideTimer = maxSlideTime;
    }

    private void SlidingMovement()
    {
        //Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        Vector3 inputDirection = orientation.forward * input.MovementInput.y + orientation.right * input.MovementInput.x;

        // sliding normal
        if (!pm.OnSlope() || rb.velocity.y > -0.1f)
        {
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);

            slideTimer -= Time.deltaTime;
        }

        // sliding down a slope
        else
        {
            rb.AddForce(pm.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
        }

        if (slideTimer <= 0)
            StopSlide();
    }

    private void StopSlide()
    {
        

        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
    }
    private void gravity()
    {
        // Apply gravity
        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }
}