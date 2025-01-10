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

    private bool readytoThrow;

    private InputHandler input;

    private void Awake()
    {
        input = GetComponent<InputHandler>();
        input.RegisterOnAimPress(prepareThrow);
        input.RegisterOnAimCancel(checkThrow);
    }

    private void Start()
    {
        readytoThrow = true;
    }

    private void prepareThrow()
    {
        if (readytoThrow)
        {
            currentThrowForce++;
            if (currentThrowForce > maxThrowForce)
            {
                currentThrowForce = maxThrowForce;
            }
        }
    }

    private void checkThrow()
    {
        if (readytoThrow)
        {
            performThrow();
            currentThrowForce = minthrowForce;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        // switch to check prepare throw
        //if (Gamepad.current.rightTrigger.isPressed && readytoThrow)
        //{
        //    currentThrowForce += 20 * Time.deltaTime;
        //    if (currentThrowForce > maxThrowForce)
        //    {
        //        currentThrowForce = maxThrowForce;
        //    }
        //}
        // switch to check throw
        //if (Gamepad.current.rightTrigger.wasReleasedThisFrame && readytoThrow)
        //{
        //    performThrow();
        //    currentThrowForce = minthrowForce;

        //    //if (Input.GetKey(KeyCode.Mouse0))
        //    //{
        //    //    throwForce++;
        //    //    if (throwForce > maxThrowForce)
        //    //    {
        //    //        throwForce = maxThrowForce;
        //    //    }

        //    //}
        //    //else
        //    //{
        //    //    throwForce--;
        //    //    if (throwForce < 1)
        //    //    {
        //    //        throwForce = 1;
        //    //    }
        //    //}

        //    //if (Input.GetKeyUp(KeyCode.Mouse0) && readytoThrow)
        //    //{
        //    //    Throw();

        //    //}
        //}
    }

    private void performThrow()
    {
        readytoThrow = false;

        GameObject projectile = Instantiate(throwing, attackPoint.position, cam.rotation);

        Rigidbody projectilerb = projectile.GetComponent<Rigidbody>();

        Vector3 forceDirection = cam.transform.forward;

        RaycastHit hit;

        if (Physics.Raycast(cam.position, cam.forward, out hit, 1000f))
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