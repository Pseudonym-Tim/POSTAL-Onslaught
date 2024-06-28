using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple camera shake that uses perlin noise...
/// </summary>
public class CameraShaker : Singleton<CameraShaker>
{
    private static CameraShakeInfo currentShakeInfo;
    private static float shakeEndTime;
    private static Vector3 initialPosition;
    private static Transform shakeTransform;

    private void Awake()
    {
        shakeTransform = transform;
        initialPosition = shakeTransform.localPosition;
    }

    public static void Shake(CameraShakeInfo newCameraShakeInfo)
    {
        currentShakeInfo = newCameraShakeInfo;
        shakeEndTime = Time.time + currentShakeInfo.shakeDuration;
        initialPosition = shakeTransform.localPosition;
    }

    private void Update()
    {
        if(currentShakeInfo != null)
        {
            if(Time.time < shakeEndTime)
            {
                UpdateShake();
            }
            else
            {
                EndShake();
            }
        }
    }

    private void UpdateShake()
    {
        float shakeProgress = (shakeEndTime - Time.time) / currentShakeInfo.shakeDuration;
        float x = Mathf.PerlinNoise(Time.time * currentShakeInfo.shakeSpeed, 0) * 2f - 1f;
        float y = Mathf.PerlinNoise(Time.time * currentShakeInfo.shakeSpeed, 1) * 2f - 1f;

        Vector3 shakeOffset = new Vector3(x, y, 0) * currentShakeInfo.shakeIntensity;
        shakeTransform.localPosition = initialPosition + shakeOffset * shakeProgress;
    }

    private void EndShake()
    {
        shakeTransform.localPosition = initialPosition;
        currentShakeInfo = null;
    }
}

[System.Serializable]
public class CameraShakeInfo
{
    public float shakeIntensity = 0.7f;
    public float shakeSpeed = 25f;
    public float shakeDuration = 0.5f;
}
