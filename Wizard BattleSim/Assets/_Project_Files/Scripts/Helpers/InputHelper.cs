using UnityEngine;
using UnityEngine.InputSystem;

public static class InputHelper
{
    public static Vector2 GetVector2Input(InputAction action)
    {
        return action.ReadValue<Vector2>();
    }

    public static float GetFloatInput(InputAction action)
    {
        return action.ReadValue<float>();
    }

    public static bool IsPressed(InputAction action)
    {
        return action.ReadValue<float>() > 0.1f;
    }

    public static bool WasPressedThisFrame(InputAction action)
    {
        return action.triggered;
    }
}
