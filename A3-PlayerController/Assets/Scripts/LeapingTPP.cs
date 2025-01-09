using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LeapingTPP : MonoBehaviour
{
    public Rigidbody rb;
    private float leapCD;
    public float leapMaxCD;
    public float leapForce;
    public PlayerMovementAdvanced pm;
    private float horizontalMovement;
    private float verticalMovement;
    public Transform orientation;
    public Transform target;
   
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovementAdvanced>();
        leapCD = 0;
        
    }

    // Update is called once per frame
    void Update()
    {
        
        horizontalMovement = Input.GetAxis("Horizontal");
        verticalMovement = Input.GetAxis("Vertical");
        if ((Gamepad.current.rightShoulder.wasPressedThisFrame) && (horizontalMovement != 0 || verticalMovement != 0) && (leapCD <= 0)) 
        {
            Debug.Log("Movement Leap");
            MovementDash();
            leapCD = leapMaxCD;
        }
        else if(Gamepad.current.rightShoulder.wasPressedThisFrame && (leapCD <= 0)) 
        {
            Debug.Log("Camera Leap");
            Dash();
            leapCD = leapMaxCD;
        }
        leapCD -= Time.deltaTime;
        if (leapCD < 0) leapCD = 0;
    }

    public void MovementDash()
    {
        rb.drag = 0;
        Vector3 inputDirection = orientation.forward * verticalMovement + orientation.right * horizontalMovement;
        

        
        rb.AddForce(inputDirection * leapForce,ForceMode.Impulse);
    }

    private void Dash()
    {
        rb.drag = 0;
        Vector3 inputDirection = target.forward;



        rb.AddForce(inputDirection * leapForce, ForceMode.Impulse);
    }


}
