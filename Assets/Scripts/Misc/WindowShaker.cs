using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// Handles the shaking of the game window...
/// </summary>
public class WindowShaker : Singleton<WindowShaker>
{
    private static float traumaAmount;
    private static float shakeSeed;
    private static RECT windowRect;
    private static IntPtr gameWindowPointer;
    private static int windowWidth, windowHeight;
    private static int windowOriginX, windowOriginY;

    public static MonoBehaviour MonoBehaviour { get; private set; } = null;
    public static bool IsShaking { get; private set; } = false;
    public static bool IsEnabled { get; set; } = true;

    private void Awake()
    {
        MonoBehaviour = this;
        gameWindowPointer = FindWindow(null, GameManager.GAME_NAME);
        GetWindowRect(gameWindowPointer, out windowRect);
        windowOriginX = windowRect.Left;
        windowOriginY = windowRect.Top;
        windowWidth = windowRect.Right - windowRect.Left;
        windowHeight = windowRect.Bottom - windowRect.Top;
        LoadWindowShakeSetting();
    }

    public static void Shake(WindowShakeInfo shakeInfo)
    {
        if(!Screen.fullScreen && !Application.isEditor && IsEnabled)
        {
            IsShaking = true;
            MonoBehaviour.StopCoroutine(nameof(ShakeCoroutine));
            MonoBehaviour.StartCoroutine(ShakeCoroutine(shakeInfo));
        }
    }

    public static void SetWindowShakeEnabled(bool isEnabled)
    {
        IsEnabled = isEnabled;
        PlayerPrefs.SetInt(OptionsUI.WINDOW_SHAKE_PREF_KEY, isEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadWindowShakeSetting()
    {
        IsEnabled = PlayerPrefs.GetInt(OptionsUI.WINDOW_SHAKE_PREF_KEY, 1) == 1;
    }

    private static IEnumerator ShakeCoroutine(WindowShakeInfo shakeInfo)
    {
        float shakeIntensityScaled = shakeInfo.shakeIntensity * 100;
        shakeSeed = UnityEngine.Random.value;
        traumaAmount = Mathf.Clamp01(traumaAmount + shakeIntensityScaled);

        float startTime = Time.time;

        // Get window rect and update origin...
        GetWindowRect(gameWindowPointer, out windowRect);
        windowOriginX = windowRect.Left;
        windowOriginY = windowRect.Top;

        while(Time.time < startTime + shakeInfo.shakeDuration)
        {
            float shakePosX = shakeIntensityScaled * (Mathf.PerlinNoise(shakeSeed, Time.time * shakeInfo.shakeSpeed) * 2 - 1);
            float shakePosY = shakeIntensityScaled * (Mathf.PerlinNoise(shakeSeed + 1, Time.time * shakeInfo.shakeSpeed) * 2 - 1);

            Vector2 shakePos = new Vector2(shakePosX, shakePosY) * traumaAmount;
            int shakeX = windowOriginX + Mathf.RoundToInt(shakePos.x);
            int shakeY = windowOriginY + Mathf.RoundToInt(shakePos.y);

            MoveWindow(gameWindowPointer, shakeX, shakeY, windowWidth, windowHeight, true);

            traumaAmount = Mathf.Clamp01(traumaAmount - (Time.deltaTime / shakeInfo.shakeDuration)); // Recover from trauma
            yield return null; // Wait for the next frame
        }

        // Reset window position after shaking is done
        MoveWindow(gameWindowPointer, windowOriginX, windowOriginY, windowWidth, windowHeight, true);
        IsShaking = false;
    }

    [DllImport("user32.dll", EntryPoint = "FindWindow")]
    private static extern IntPtr FindWindow(string className, string windowName);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left, Top, Right, Bottom;
    }
}

[Serializable]
public class WindowShakeInfo
{
    public float shakeIntensity = 0.1f;
    public float shakeDuration = 1f;
    public float shakeSpeed = 1f;
}