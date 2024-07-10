using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Handles everything related to hurt flashing...
/// </summary>
[RequireComponent(typeof(Entity))]
public class HurtFlash : MonoBehaviour
{
    private static readonly int HURT_FLASH_PROPERTY = Shader.PropertyToID("_HurtFlash");
    private const float DEFAULT_FLASH_TIME = 0.1f;
    private List<SpriteRenderer> flashSpriteRenderers;

    public static void ApplyHurtFlash(Entity entity, float flashTime = DEFAULT_FLASH_TIME)
    {
        HurtFlash hurtFlash = entity.GetComponent<HurtFlash>() ?? entity.AddComponent<HurtFlash>();
        hurtFlash.Setup(flashTime);
    }

    public void Setup(float flashTime = DEFAULT_FLASH_TIME)
    {
        flashSpriteRenderers = new List<SpriteRenderer>();
        SpriteRenderer[] spriteRendererList = GetComponentsInChildren<SpriteRenderer>();

        foreach(SpriteRenderer spriteRenderer in spriteRendererList)
        {
            if(spriteRenderer.material.HasProperty(HURT_FLASH_PROPERTY))
            {
                flashSpriteRenderers.Add(spriteRenderer);
            }
        }

        if(flashSpriteRenderers.Count > 0)
        {
            StopAllCoroutines();
            StartCoroutine(PerformFlash(flashTime));
        }
        else
        {
            Destroy(this);
        }
    }

    private IEnumerator PerformFlash(float flashTime)
    {
        SetHurtFlash(1f);
        yield return new WaitForSeconds(flashTime);
        SetHurtFlash(0f);
        Destroy(this);
    }

    private void SetHurtFlash(float value)
    {
        foreach(SpriteRenderer spriteRenderer in flashSpriteRenderers)
        {
            Material material = spriteRenderer.material;
            material.SetFloat(HURT_FLASH_PROPERTY, value);
        }
    }
}
