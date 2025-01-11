using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    private InputActionMap playerControls;
    public bool HasInputActionMap => playerControls != null;

    private Vector2 movementInput;
    public Vector2 MovementInput => movementInput;

    private Vector2 lookInput;
    public Vector2 LookInput => lookInput;

    private event Action onJumpPressed;

    private event Action onAimPress;

    private event Action onAimCancel;

    private event Action onSprintPress;

    private event Action onSprintCancel;

    private event Action onCrouchPress;

    private event Action onCrouchCancel;

    private event Action onSlidePress;

    private event Action onSlideCancel;

    private event Action onDashPressed;

    private class ActionName
    {
        public const string Move = "Move";
        public const string Look = "Look";
        public const string Jump = "Jump";
        public const string Sprint = "Sprint";
        public const string Aim = "Aim";
        public const string Crouch = "Crouch";
        public const string Slide = "Slide";
        public const string Dash = "Dash";
    }

    public void SetPlayerInput(PlayerInput input)
    {
        playerControls = input.actions.FindActionMap("Player");

        playerControls.FindAction(ActionName.Move).performed += i => movementInput = i.ReadValue<Vector2>();
        playerControls.FindAction(ActionName.Look).performed += i => lookInput = i.ReadValue<Vector2>();

        playerControls.FindAction(ActionName.Jump).performed += _ => invokeJumpPressed();

        playerControls.FindAction(ActionName.Sprint).started += _ => handleSprintInput(InputActionPhase.Started);
        playerControls.FindAction(ActionName.Sprint).canceled += _ => handleSprintInput(InputActionPhase.Canceled);

        playerControls.FindAction(ActionName.Aim).started += _ => handleAimInput(InputActionPhase.Started);
        playerControls.FindAction(ActionName.Aim).canceled += _ => handleAimInput(InputActionPhase.Canceled);

        playerControls.FindAction(ActionName.Crouch).started += _ => handleCrouchInput(InputActionPhase.Started);
        playerControls.FindAction(ActionName.Crouch).canceled += _ => handleCrouchInput(InputActionPhase.Canceled);

        playerControls.FindAction(ActionName.Slide).started += _ => handleSlideInput(InputActionPhase.Started);
        playerControls.FindAction(ActionName.Slide).canceled += _ => handleSlideInput(InputActionPhase.Canceled);

        playerControls.FindAction(ActionName.Dash).performed += _ => handleDashInput();

        foreach (var cam in _storedCams)
        {
            setupCinemachineControl(cam);
        }
        _storedCams.Clear();
    }

    public void SetEnable(bool value)
    {
        if (value) playerControls.Enable();
        else playerControls.Disable();
    }

    private List<Cinemachine.CinemachineVirtualCamera> _storedCams = new List<Cinemachine.CinemachineVirtualCamera>();

    public void SetupCinemachineCameraControl(Cinemachine.CinemachineVirtualCamera camera)
    {
        if (playerControls != null)
        {
            setupCinemachineControl(camera);
        }
        else
        {
            _storedCams.Add(camera);
        }
    }

    private void setupCinemachineControl(Cinemachine.CinemachineVirtualCamera camera)
    {
        var customInput = camera.gameObject.GetComponent<CustomCameraInputProviderForCinemachine>();
        customInput.Init(playerControls.FindAction(ActionName.Look));
    }

    #region Jump

    public void RegisterOnJumpInput(Action action)
    {
        onJumpPressed += action;
    }

    private void invokeJumpPressed()
    {
        //log("jump press");
        onJumpPressed?.Invoke();
    }

    #endregion Jump

    #region Crouching

    public void RegisterOnCrouchPress(Action action)
    {
        onCrouchPress += action;
    }

    public void RegisterOnCrouchCancel(Action action)
    {
        onCrouchCancel += action;
    }

    private void handleCrouchInput(InputActionPhase phase)
    {
        if (phase == InputActionPhase.Started)
        {
            //log("Crouch press");
            onCrouchPress?.Invoke();
        }
        if (phase == InputActionPhase.Canceled)
        {
            //log("Crouch cancel");
            onCrouchCancel?.Invoke();
        }
    }

    #endregion Crouching

    #region Sprint

    public void RegisterOnSprintPress(Action action)
    {
        onSprintPress += action;
    }

    public void RegisterOnSpiritCancel(Action action)
    {
        onSprintCancel += action;
    }

    private void handleSprintInput(InputActionPhase phase)
    {
        if (phase == InputActionPhase.Started)
        {
            onSprintPress?.Invoke();
        }
        if (phase == InputActionPhase.Canceled)
        {
            onSprintCancel?.Invoke();
        }
    }

    #endregion Sprint

    #region Aim

    public void RegisterOnAimPress(Action action)
    {
        onAimPress += action;
    }

    public void RegisterOnAimCancel(Action action)
    {
        onAimCancel += action;
    }

    private void handleAimInput(InputActionPhase phase)
    {
        if (phase == InputActionPhase.Started)
        {
            onAimPress?.Invoke();
        }
        if (phase == InputActionPhase.Canceled)
        {
            onAimCancel?.Invoke();
        }
    }

    #endregion Aim

    #region Slide

    public void RegisterOnSlidePress(Action action)
    {
        onSlidePress += action;
    }

    public void RegisterOnSlideCancel(Action action)
    {
        onSlideCancel += action;
    }

    private void handleSlideInput(InputActionPhase phase)
    {
        if (phase == InputActionPhase.Started)
        {
            log("Slide press");
            onSlidePress?.Invoke();
        }
        if (phase == InputActionPhase.Canceled)
        {
            log("Slide press");
            onSlideCancel?.Invoke();
        }
    }

    #endregion Slide

    #region Dash

    public void RegisterOnDashPressed(Action action)
    {
        
        onDashPressed += action;
    }

    private void handleDashInput()
    {
        log("Dash press");
        onDashPressed?.Invoke();
    }

    #endregion Dash

    private void log(string s)
    {
#if UNITY_EDITOR
        Debug.Log(s);
#endif
    }
}