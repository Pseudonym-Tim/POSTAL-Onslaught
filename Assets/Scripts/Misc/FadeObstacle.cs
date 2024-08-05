using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the fading of obstacles based on player/NPC proximity.
/// </summary>
public class FadeObstacle : MonoBehaviour
{
    [Header("Overlap Box Settings")]
    public Vector2 boxOrigin = Vector2.zero;
    public Vector2 boxSize = Vector2.one;

    [Header("Fading Settings")]
    public SpriteRenderer spriteRenderer;
    public float fadeSpeed = 2f;
    public float targetAlpha = 0.5f;

    private float originalAlpha;
    private bool isFadingOut = false;

    private void Awake()
    {
        originalAlpha = spriteRenderer.color.a;
    }

    private void LateUpdate()
    {
        CheckOverlap();
        HandleFading();
    }

    private void CheckOverlap()
    {
        Vector2 origin = (Vector2)transform.position + boxOrigin;
        Collider2D playerHit = Physics2D.OverlapBox(origin, boxSize, 0, LayerManager.Masks.PLAYER);
        Collider2D npcHit = Physics2D.OverlapBox(origin, boxSize, 0, LayerManager.Masks.NPC);

        if(playerHit != null || npcHit != null)
        {
            isFadingOut = true;
            spriteRenderer.sortingOrder = 3;
        }
        else
        {
            isFadingOut = false;
            spriteRenderer.sortingOrder = 2;
        }
    }

    private void HandleFading()
    {
        Color color = spriteRenderer.color;
        float targetAlphaValue = isFadingOut ? targetAlpha : originalAlpha;
        color.a = Mathf.Lerp(color.a, targetAlphaValue, fadeSpeed * Time.deltaTime);
        spriteRenderer.color = color;
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 origin = (Vector2)transform.position + boxOrigin;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(origin, boxSize);
    }
}
