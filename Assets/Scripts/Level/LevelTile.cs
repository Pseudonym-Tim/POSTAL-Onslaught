using UnityEngine;

/// <summary>
/// Level tile...
/// </summary>
public class LevelTile : MonoBehaviour
{
    private SpriteRenderer tileGFX;

    public void OnTileSpawn()
    {
        tileGFX = gameObject.AddComponent<SpriteRenderer>();
        tileGFX.sortingOrder = TileData.sortingOrder;
        int randomIndex = Random.Range(0, TileData.sprites.Count);
        tileGFX.sprite = TileData.sprites[randomIndex];

        if(TileData.addCollision)
        {
            BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
            boxCollider.size = tileGFX.bounds.size;
        }
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

    public Vector3 TilePosition { get { return transform.position; } set { transform.position = value; } }

    public TileData TileData { get; set; } = null;
}
