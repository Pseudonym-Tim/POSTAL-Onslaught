using UnityEngine;

/// <summary>
/// Handles everything related to managing tiles...
/// </summary>
public class TileManager : Singleton<TileManager>
{ 
    public static readonly float TILE_SIZE = 1f;

    [SerializeField] private TileDatabase tileDatabase;
    [SerializeField] private PrefabDatabase prefabDatabase;

    public LevelTile AddTile(string tileID, Vector2 tilePos)
    {
        TileData tileData = tileDatabase.GetTile(tileID);
        Transform levelParent = LevelGenerator.TileParent.transform;
        return LevelTile.Create(tileData, tilePos, levelParent);
    }

    public void RemoveTile(Vector2 tilePos)
    {
        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        LevelTile tileToDestroy = levelManager.LevelTiles[tilePos];
        Destroy(tileToDestroy);
    }

    public static Vector2 SnapToGrid(Vector2 pos)
    {
        float x = Mathf.Round(pos.x / TILE_SIZE) * TILE_SIZE;
        float y = Mathf.Round(pos.y / TILE_SIZE) * TILE_SIZE;
        return new Vector2(x, y);
    }
}
