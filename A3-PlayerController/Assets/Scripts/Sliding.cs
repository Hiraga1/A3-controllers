using UnityEngine;

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
        //quick fix, should not do like this in real project
        pm.sliding = true;
        //end quick fix

        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
        rb.AddForce(Vector3.down * 10f, ForceMode.Impulse);

        slideTimer = maxSlideTime;
    }

    private void SlidingMovement()
    {

        if (pm.sliding)
        {// sliding normal
            if (!pm.OnSlope() || rb.velocity.y > -0.1f)
            {
                rb.AddForce(pm.MoveDirection.normalized * slideForce, ForceMode.Force);

                slideTimer -= Time.deltaTime;
            }

            // sliding down a slope
            else
            {
                rb.AddForce(pm.GetSlopeMoveDirection(pm.MoveDirection) * slideForce, ForceMode.Force);
            }

            if (slideTimer <= 0)
                StopSlide();
        }
    }

    private void StopSlide()
    {
        //quick fix, should not do like this in real project
        pm.sliding = false;

        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
    }
}