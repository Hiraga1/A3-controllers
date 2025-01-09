using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Throwing : MonoBehaviour
{
    public Transform cam;
    
    public Transform attackPoint;
    
    public GameObject throwing;

    public float CD;

    public float minthrowForce;
    public float maxThrowForce;
    public float upwardThrowForce;
    private float currentThrowForce;

    bool readytoThrow;


    void Start()
    {
        readytoThrow = true;


    }

    // Update is called once per frame
    private void Update()
    {
        
        if (Gamepad.current.rightTrigger.isPressed && readytoThrow)
        {
            currentThrowForce += 20 * Time.deltaTime;
            if (currentThrowForce > maxThrowForce)
            {
                currentThrowForce = maxThrowForce;
            }
           

        }
        if (Gamepad.current.rightTrigger.wasReleasedThisFrame && readytoThrow)
        {
            Throw();
            currentThrowForce = minthrowForce;

            //if (Input.GetKey(KeyCode.Mouse0))
            //{

            //    throwForce++;
            //    if (throwForce > maxThrowForce)
            //    {
            //        throwForce = maxThrowForce;
            //    }

            //}
            //else
            //{
            //    throwForce--;
            //    if (throwForce < 1)
            //    {
            //        throwForce = 1;
            //    }
            //}


            //if (Input.GetKeyUp(KeyCode.Mouse0) && readytoThrow) 
            //{
            //    Throw();

            //}
        }
    }

    

    private void Throw()
    {
        readytoThrow = false;

        GameObject projectile = Instantiate(throwing, attackPoint.position, cam.rotation);

        Rigidbody projectilerb = projectile.GetComponent<Rigidbody>();

        Vector3 forceDirection = cam.transform.forward;

        RaycastHit hit;

        if(Physics.Raycast(cam.position, cam.forward, out hit, 1000f))
        {
            forceDirection = (hit.point - attackPoint.position).normalized;
        }

        Vector3 forceToAdd = forceDirection * currentThrowForce + transform.up * upwardThrowForce;

        projectilerb.AddForce(forceToAdd, ForceMode.Impulse);

        Invoke(nameof(Cooldown), CD);
    }
    


    private void Cooldown()
    {
        readytoThrow = true;
    }
   
}
