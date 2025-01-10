using System.Collections;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class CustomCameraInputProviderForCinemachine : MonoBehaviour, AxisState.IInputAxisProvider
{
    private InputAction look;

    public void Init(InputAction inputAction)
    {
        look = inputAction;
    }

    public float GetAxisValue(int axis)
    {
        if (look != null)
        {
            switch (axis)
            {
                case 0: return look.ReadValue<Vector2>().x;
                case 1: return look.ReadValue<Vector2>().y;
                    //case 2: return action.ReadValue<float>();
            }
        }
        return 0;
    }
}