using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles everything related to the collision hull...
/// </summary>
public class CollisionHull2D : MonoBehaviour
{
    [SerializeField] private HullData2D hullData = new HullData2D();
    private BoxCollider2D boxCollider;

    private void Awake()
    {
        InitializeCollider();
    }

    private void InitializeCollider()
    {
        boxCollider = gameObject.AddComponent<BoxCollider2D>();
        boxCollider.offset = hullData.center;
        boxCollider.size = hullData.size;
        gameObject.layer = LayerManager.ToLayerID(hullData.collisionLayer);
    }

    public static void AddCollisionHull(GameObject targetObject, HullData2D hullData)
    {
        CollisionHull2D collisionHull = targetObject.AddComponent<CollisionHull2D>();
        collisionHull.hullData = hullData;
        collisionHull.InitializeCollider();
    }

    public static void AddCollisionHull(Entity entity, HullData2D hullData)
    {
        AddCollisionHull(entity.EntityObject, hullData);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(hullData.center, hullData.size);
    }

    [System.Serializable]
    public class HullData2D
    {
        public Vector2 center = Vector2.zero;
        public Vector2 size = Vector2.one;
        public LayerMask collisionLayer = LayerManager.Layers.DEFAULT;
    }
}
