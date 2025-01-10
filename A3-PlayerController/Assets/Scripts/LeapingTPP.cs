using UnityEngine;
using UnityEngine.InputSystem;

public class LeapingTPP : MonoBehaviour
{
    public Rigidbody rb;
    private float leapCD;
    public float leapMaxCD;
    public float leapForce;
    public PlayerMovement pm;
    //private float horizontalMovement;
    //private float verticalMovement;
    public Transform orientation;
    public Transform target;

    private InputHandler input;

    private void Awake()
    {
        input = GetComponent<InputHandler>();
        input.RegisterOnDashPressed(checkDash);
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
        leapCD = 0;
    }

    private void checkDash()
    {
        if (input.MovementInput != Vector2.zero && leapCD <= 0)
        {
            MovementDash();
            leapCD = leapMaxCD;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        //horizontalMovement = Input.GetAxis("Horizontal");
        //verticalMovement = Input.GetAxis("Vertical");
        //if ((Gamepad.current.rightShoulder.wasPressedThisFrame) && (horizontalMovement != 0 || verticalMovement != 0) && (leapCD <= 0))
        //{
        //    Debug.Log("Movement Leap");
        //    MovementDash();
        //    leapCD = leapMaxCD;
        //}
        //else if (Gamepad.current.rightShoulder.wasPressedThisFrame && (leapCD <= 0))
        //{
        //    Debug.Log("Camera Leap");
        //    Dash();
        //    leapCD = leapMaxCD;
        //}
        leapCD -= Time.deltaTime;
        if (leapCD < 0) leapCD = 0;
    }

    public void MovementDash()
    {
        rb.drag = 0;
        //Vector3 inputDirection = orientation.forward * verticalMovement + orientation.right * horizontalMovement;
        Vector3 inputDirection = orientation.forward * input.MovementInput.y + orientation.right * input.MovementInput.x;

        rb.AddForce(inputDirection * leapForce, ForceMode.Impulse);
    }

    private void Dash()
    {
        rb.drag = 0;
        Vector3 inputDirection = target.forward;

        rb.AddForce(inputDirection * leapForce, ForceMode.Impulse);
    }
}