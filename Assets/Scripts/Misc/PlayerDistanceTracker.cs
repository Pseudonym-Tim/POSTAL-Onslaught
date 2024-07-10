using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles distance tracking...
/// </summary>
public class PlayerDistanceTracker : MonoBehaviour
{
    private Entity playerEntity;
    private Vector2 lastPosition;
    private float totalDistance;

    public void Setup()
    {
        playerEntity = GetComponent<Player>();
        lastPosition = playerEntity.EntityPosition;
        totalDistance = 0f;
    }

    private void Update()
    {
        TrackDistance();
    }

    private void TrackDistance()
    {
        // Calculate the distance covered since the last frame...
        float distanceCovered = Vector2.Distance(playerEntity.EntityPosition, lastPosition);
        totalDistance += distanceCovered;
        lastPosition = playerEntity.EntityPosition;
        GameManager.GameStats.DistanceCovered = totalDistance;
    }
}
