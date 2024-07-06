using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles overall input management related stuff...
/// </summary>
public class InputManager : Singleton<InputManager>
{
    private static Dictionary<string, string> buttonStrings;

    public static bool IsButtonPressed(string buttonName) => Input.GetButtonDown(buttonName);
    public static float GetAxis(string axisName) => Input.GetAxis(axisName);
    public static float GetAxisRaw(string axisName) => Input.GetAxisRaw(axisName);
    public static bool IsButtonHeld(string buttonName) => Input.GetButton(buttonName);

    private void Awake()
    {
        buttonStrings = new Dictionary<string, string>();
        LockCursor(false);
        ShowHardwareCursor(false);
        RegisterButtonString("Interact", "F");
    }

    public static void EnableInput()
    {
        PlayerInput.InputEnabled = true;
    }

    public static void DisableInput()
    {
        PlayerInput.InputEnabled = false;
    }

    public static void LockCursor(bool lockCursor = false)
    {
        Cursor.lockState = lockCursor ? CursorLockMode.Locked : CursorLockMode.Confined;
    }

    public static void ShowHardwareCursor(bool showCursor = true)
    {
        Cursor.visible = showCursor;
    }

    public static void RegisterButtonString(string buttonName, string displayString)
    {
        if(!buttonStrings.ContainsKey(buttonName))
        {
            buttonStrings.Add(buttonName, displayString);
        }
        else
        {
            buttonStrings[buttonName] = displayString;
        }
    }

    public static string GetButtonString(string buttonName)
    {
        if(buttonStrings.TryGetValue(buttonName, out string displayString))
        {
            return displayString;
        }

        return string.Empty;
    }
}
