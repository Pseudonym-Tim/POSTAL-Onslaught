using UnityEngine;

/// <summary>
/// Handles everything relatd to the vortex UI effect...
/// </summary>
public class VortexEffectUI : MonoBehaviour
{
    public Material material;
    public float speed = 1.0f;

    private void Update()
    {
        if(material != null)
        {
            float timeParam = Time.unscaledTime;
            material.SetFloat("_TimeParam", timeParam);
            material.SetFloat("_SpeedParam", speed);
        }
    }
}
