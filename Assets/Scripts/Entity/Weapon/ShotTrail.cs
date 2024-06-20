using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Shot trail for ranged weapons...
/// </summary>
public class ShotTrail : MonoBehaviour
{
    private const float DESTROY_TIME = 0.5f;

    [SerializeField] private float shotTrailSpeed = 0.01f;
    private float shotTrailTimer = 0;
    private Vector3 startPosition;

    public void Awake()
    {
        startPosition = transform.position;
        Destroy(gameObject, DESTROY_TIME);
    }

    private void Update()
    {
        if(EndPosition != Vector3.zero)
        {
            shotTrailTimer += Time.deltaTime * shotTrailSpeed;
            transform.position = Vector3.Lerp(startPosition, EndPosition, shotTrailTimer);
        }
    }

    public Vector3 EndPosition { get; set; } = Vector3.zero;
}
