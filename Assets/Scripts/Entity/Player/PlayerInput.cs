using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles everything related to player input...
/// </summary>
public class PlayerInput : Singleton<PlayerInput>
{
    public static bool IsButtonPressed(string buttonName) => InputManager.IsButtonPressed(buttonName) && InputEnabled;
    public static bool IsButtonHeld(string buttonName) => InputManager.IsButtonHeld(buttonName) && InputEnabled;
    public static float GetAxis(string axisName) => InputEnabled ? InputManager.GetAxis(axisName) : 0;
    public static float GetAxisRaw(string axisName) => InputEnabled ? InputManager.GetAxisRaw(axisName) : 0;

    // Weapon scrolling...
    public static bool WeaponScrollRight { get { return GetAxis("Mouse ScrollWheel") > 0f; } }
    public static bool WeaponScrollLeft { get { return GetAxis("Mouse ScrollWheel") < 0f; } }

    public static bool Interact { get { return IsButtonPressed("Interact"); } }

    // Weapon slot hotkeys...
    public static bool WeaponSlot1 { get { return IsButtonPressed("WeaponSlot1"); } }
    public static bool WeaponSlot2 { get { return IsButtonPressed("WeaponSlot2"); } }
    public static bool WeaponSlot3 { get { return IsButtonPressed("WeaponSlot3"); } }

    public static bool InputEnabled { get; set; } = false;
}
