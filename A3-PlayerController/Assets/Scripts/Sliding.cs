using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class Sliding : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;    
    public Transform playerObj;
    private Rigidbody rb;
    private PlayerMovementAdvanced pm;
    private InputHandler input;

    [Header("Sliding")]
    public float maxSlideTime;
    public float slideForce;
    private float slideTimer;

    public float slideYScale;
    private float startYScale;

    private void Awake()
    {
        input = GetComponent<InputHandler>();
        input.RegisterOnSlidePress(StartSlide);
        input.RegisterOnSlideCancel(StopSlide);
    }
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovementAdvanced>();
        startYScale = playerObj.localScale.y;
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
        pm.sliding = false;

        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
    }
   
}