using NavMeshPlus.Components;
using UnityEngine;

/// <summary>
/// Level tile...
/// </summary>
public class LevelTile : MonoBehaviour
{
    private SpriteRenderer tileGFX;
    private NavMeshModifier navMeshModifier;

    public void OnTileSpawn()
    {
        tileGFX = TileObject.AddComponent<SpriteRenderer>();
        navMeshModifier = TileObject.AddComponent<NavMeshModifier>();
        tileGFX.sortingOrder = TileData.sortingOrder;
        tileGFX.sortingLayerID = LayerManager.SortingLayers.DEFAULT;
        int randomIndex = Random.Range(0, TileData.sprites.Count);
        tileGFX.sprite = TileData.sprites[randomIndex];

        if(TileData.collisionData.addCollision)
        {
            BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
            boxCollider.size = TileData.collisionData.size;
            boxCollider.offset = TileData.collisionData.offset;
            boxCollider.compositeOperation = Collider2D.CompositeOperation.Merge;
            navMeshModifier.area = 1;
        }

        navMeshModifier.area = TileData.navigationData.area;
        navMeshModifier.overrideArea = TileData.navigationData.overrideArea;
    }

    /// <summary>
    /// Create a new level tile...
    /// </summary>
    public static LevelTile Create(TileData tileData, Vector2 tilePos, Transform parent)
    {
        GameObject newTile = new GameObject(tileData.name);
        LevelTile levelTile = newTile.AddComponent<LevelTile>();
        levelTile.TileData = tileData;
        levelTile.name = tileData.name;
        levelTile.TilePosition = tilePos;
        levelTile.SetParent(parent, true);
        levelTile.OnTileSpawn();
        return levelTile;
    }

    public void SetParent(Transform parentTransform, bool worldPositionStays = true)
    {
        transform.SetParent(parentTransform, worldPositionStays);
    }

    public void DestroyTile()
    {
        Destroy(TileObject);
    }

    public Vector3 TilePosition { get { return transform.position; } set { transform.position = value; } }
    public GameObject TileObject { get { return gameObject; } }
    public TileData TileData { get; set; } = null;
}
