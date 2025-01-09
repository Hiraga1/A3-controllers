﻿using System.Collections;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    private Transform cameraTransform;
    private Vector2 movementInput;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private bool jumpInput;
    private bool isAiming;
    public bool isspirit;

    [SerializeField]
    private InputActionReference movementControl;
    [SerializeField]
    private InputActionReference spiritControl;
    [SerializeField]
    private InputActionReference jumpControl;
    [SerializeField]
    private InputActionReference aimControl;


    [SerializeField]
    private float playerSpeed = 2.0f;
    [SerializeField]
    private float spiritMulti = 1.5f;
    [SerializeField]
    private float rotationSpeed = 4.0f;
    [SerializeField]
    private float jumpHeight = 1.0f;
    [SerializeField]
    private float gravityValue = -9.81f;

    // Cinemachine FreeLook Camera (for normal movement)
    [SerializeField]
    private CinemachineVirtualCamera freeLookCam;
    // Cinemachine Virtual Camera (for aiming)
    [SerializeField]
    private CinemachineVirtualCamera aimCamera;
    CinemachineBasicMultiChannelPerlin channelPerlin;

    // Duration for the camera transition (seconds)
    [SerializeField]
    private float transitionDuration = 0.5f;
    [SerializeField]
    private GameObject Crosshair;

    private float currentLerpTime;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraTransform = Camera.main != null ? Camera.main.transform : throw new System.Exception("Main Camera not found!");
        channelPerlin = freeLookCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        // Set initial priorities
        freeLookCam.Priority = 10;
        aimCamera.Priority = 5;
    }

    private void OnEnable()
    {
        movementControl.action.Enable();
        jumpControl.action.Enable();
        aimControl.action.Enable();
        spiritControl.action.Enable();
    }

    private void OnDisable()
    {
        movementControl.action.Disable();
        jumpControl.action.Disable();
        aimControl.action.Disable();
        spiritControl.action.Disable();
    }

    void Update()
    {
        // Handle input
        movementInput = movementControl.action.ReadValue<Vector2>();
        jumpInput = jumpControl.action.triggered;
        isAiming = aimControl.action.IsPressed();
        isspirit = spiritControl.action.IsPressed();

        // Switch between normal camera and aim camera
        SwitchCamera();

        // Handle movement
        MovePlayer();

        // Handle jumping
        Jump();

        HandleFootstepsAndShake();

        // Update gravity
        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
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

    private void HandleFootstepsAndShake()
    {
        if (groundedPlayer && movementInput.magnitude > 0.1f) // Only play footsteps when moving
        {
            channelPerlin.m_AmplitudeGain = 1f;
            channelPerlin.m_FrequencyGain = 2f;
            
        }
        else
        {
            channelPerlin.m_AmplitudeGain = 0f;
            channelPerlin.m_FrequencyGain = 0f;
        }
    }

    private void MovePlayer()
    {
        groundedPlayer = controller.isGrounded;  // Check if the player is grounded

        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f; // Reset the vertical velocity when grounded
        }

        

        if (!isAiming)
        {
            // Regular movement when not aiming
            Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
            Vector3 move = forward * movementInput.y + right * movementInput.x;
            float currentspeed = isspirit ? spiritMulti * playerSpeed : playerSpeed;
            controller.Move(move * Time.deltaTime * currentspeed);

            if (movementInput != Vector2.zero)
            {
                float targetAngle = Mathf.Atan2(movementInput.x, movementInput.y) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
                Quaternion rotation = Quaternion.Euler(0f, targetAngle, 0f);
                transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);
            }
        }
        else
        {
            // Slow down movement when aiming
            Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
            Vector3 move = forward * movementInput.y + right * movementInput.x;
            controller.Move(move * Time.deltaTime * (playerSpeed / 2)); // slower speed while aiming
        }
    }

    private void Jump()
    {
        if (jumpInput && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -2.0f * gravityValue);  // Apply jump force
        }

        // Apply gravity
        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }


   

    private void OnDrawGizmos()
    {
        if (cameraTransform != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, cameraTransform.forward * 2f);
        }
    }
}
