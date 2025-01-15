using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    private InputActionMap playerControls;
    public bool HasInputActionMap => playerControls != null;
    private InputActionMap commonControl;

    private InputAction movementInput;
    public Vector2 MovementInput => movementInput != null ? movementInput.ReadValue<Vector2>() : Vector2.zero;

    private InputAction lookInput;
    public Vector2 LookInput => lookInput != null ? lookInput.ReadValue<Vector2>() : Vector2.zero;

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
        public const string JoinOrPause = "Join";
    }

    private PlayerInput currentInput;

    public void SetPlayerInput(PlayerInput input)
    {
        if (currentInput == input) return;
        currentInput = input;
        playerControls = input.actions.FindActionMap("Player");

        movementInput = playerControls.FindAction(ActionName.Move);
        lookInput = playerControls.FindAction(ActionName.Look);

        playerControls.FindAction(ActionName.Jump).performed += invokeJumpPressed;

        playerControls.FindAction(ActionName.Sprint).started += handleSprintInput;
        playerControls.FindAction(ActionName.Sprint).canceled += handleSprintInput;

        playerControls.FindAction(ActionName.Aim).started += handleAimInput;
        playerControls.FindAction(ActionName.Aim).canceled += handleAimInput;

        playerControls.FindAction(ActionName.Crouch).started += handleCrouchInput;
        playerControls.FindAction(ActionName.Crouch).canceled += handleCrouchInput;

        playerControls.FindAction(ActionName.Slide).started += handleSlideInput;
        playerControls.FindAction(ActionName.Slide).canceled += handleSlideInput;

        playerControls.FindAction(ActionName.Dash).performed += handleDashInput;

        commonControl = input.actions.FindActionMap("Common");
        commonControl.Enable();
        commonControl.FindAction(ActionName.JoinOrPause).performed += pressPause;

        foreach (var cam in _storedCams)
        {
            setupCinemachineControl(cam);
        }
        _storedCams.Clear();
    }

    public void ClearCallbackInput()
    {
        playerControls.FindAction(ActionName.Jump).performed -= invokeJumpPressed;

        playerControls.FindAction(ActionName.Sprint).started -= handleSprintInput;
        playerControls.FindAction(ActionName.Sprint).canceled -= handleSprintInput;

        playerControls.FindAction(ActionName.Aim).started -= handleAimInput;
        playerControls.FindAction(ActionName.Aim).canceled -= handleAimInput;

        playerControls.FindAction(ActionName.Crouch).started -= handleCrouchInput;
        playerControls.FindAction(ActionName.Crouch).canceled -= handleCrouchInput;

        playerControls.FindAction(ActionName.Slide).started -= handleSlideInput;
        playerControls.FindAction(ActionName.Slide).canceled -= handleSlideInput;

        playerControls.FindAction(ActionName.Dash).performed -= handleDashInput;

        commonControl.FindAction(ActionName.JoinOrPause).performed -= pressPause;
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

    private void invokeJumpPressed(InputAction.CallbackContext context)
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

    private void handleCrouchInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            //log("Crouch press");
            onCrouchPress?.Invoke();
        }
        if (context.phase == InputActionPhase.Canceled)
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

    private void handleSprintInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            onSprintPress?.Invoke();
        }
        if (context.phase == InputActionPhase.Canceled)
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

    private void handleAimInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            onAimPress?.Invoke();
        }
        if (context.phase == InputActionPhase.Canceled)
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

    private void handleSlideInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            log("Slide press");
            onSlidePress?.Invoke();
        }
        if (context.phase == InputActionPhase.Canceled)
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

    private void handleDashInput(InputAction.CallbackContext context)
    {
        log("Dash press");
        onDashPressed?.Invoke();
    }

    #endregion Dash

    private void pressPause(InputAction.CallbackContext context)
    {
        GameManager.Instance.NotifyPlayerPressPause(this);
    }

    private void log(string s)
    {
#if UNITY_EDITOR
        Debug.Log(s);
#endif
    }
}