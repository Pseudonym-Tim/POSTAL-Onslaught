using UnityEngine;

/// <summary>
/// Handles overall input management related stuff...
/// </summary>
public class InputManager : Singleton<InputManager>
{
    public static bool IsButtonPressed(string buttonName) => Input.GetButtonDown(buttonName);
    public static float GetAxis(string axisName) => Input.GetAxis(axisName);
    public static float GetAxisRaw(string axisName) => Input.GetAxisRaw(axisName);
    public static bool IsButtonHeld(string buttonName) => Input.GetButton(buttonName);

    private void Awake()
    {
        // TODO: Disable when UI based crosshair is implemented!
        ShowCursor(true);
    }

    public static void EnableInput()
    {
        PlayerInput.InputEnabled = true;
    }

    public static void DisableInput()
    {
        PlayerInput.InputEnabled = false;
    }

    public static void ShowCursor(bool showCursor = true)
    {
        Cursor.visible = showCursor;
        Cursor.lockState = showCursor ? CursorLockMode.Confined : CursorLockMode.Locked;
    }
}