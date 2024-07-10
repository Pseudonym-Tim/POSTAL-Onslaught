using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;

/// <summary>
/// Handles everything related to the player camera...
/// </summary>
public class PlayerCamera : Entity
{
    private const int DEFAULT_ZOOM = 4;
    private const float MAX_AIM_DIST = 3.0f;

    [SerializeField] private float smoothTime = 0.3f;
    [SerializeField] private Vector2 followOffset;
    private PixelPerfectCamera pixelPerfectCamera;
    private Vector3 currentVelocity = Vector3.zero;

    public void Setup()
    {
        pixelPerfectCamera = GetComponentInChildren<PixelPerfectCamera>();
        PlayerEntity = GetComponentInParent<Player>();
        SetZoomLevel(DEFAULT_ZOOM);
        SetParent(null);
        Vector2 targetPos = (Vector2)PlayerEntity.EntityPosition + followOffset;
        EntityPosition = GetFollowPos(targetPos);
    }

    private void SetZoomLevel(int zoomLevel)
    {
        zoomLevel = Mathf.Clamp(zoomLevel, 1, 5);
        pixelPerfectCamera.refResolutionX = Mathf.FloorToInt(Screen.width / zoomLevel);
        pixelPerfectCamera.refResolutionY = Mathf.FloorToInt(Screen.height / zoomLevel);
    }

    private void LateUpdate()
    {
        if(PlayerEntity != null && PlayerEntity.IsAlive)
        {
            // Follow the player's position smoothly...
            Vector2 mousePos = GetMousePos() * MAX_AIM_DIST;
            Vector2 targetPos = (Vector2)PlayerEntity.EntityPosition + followOffset + mousePos;
            EntityPosition = Vector3.SmoothDamp(EntityPosition, GetFollowPos(targetPos), ref currentVelocity, smoothTime);
        }
    }

    private Vector2 GetMousePos()
    {
        Vector2 mousePos = Input.mousePosition;
        Vector3 viewportPos = Camera.ScreenToViewportPoint(mousePos);
        viewportPos = viewportPos * 2 - Vector3.one;
        const float MAX_POS = 0.9f;
        viewportPos.x = Mathf.Clamp(viewportPos.x, -MAX_POS, MAX_POS);
        viewportPos.y = Mathf.Clamp(viewportPos.y, -MAX_POS, MAX_POS);
        return viewportPos;
    }

    public Vector3 GetFollowPos(Vector3 targetPos)
    {
        return new Vector3(targetPos.x, targetPos.y, -10);
    }

    public Player PlayerEntity { get; set; } = null;
    public Camera Camera { get { return Camera.main; } }
}
