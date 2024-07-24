using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Handles everything related to applying decals to level tiles...
/// </summary>
public class DecalManager : Singleton<DecalManager>
{
    public List<DecalData> decalList;
    private static LevelManager levelManager;
    private static Dictionary<string, DecalData> decalDatabase;

    public void Setup()
    {
        levelManager = FindFirstObjectByType<LevelManager>();
        decalDatabase = new Dictionary<string, DecalData>();
        decalDatabase = decalList.ToDictionary(decal => decal.decalID);
    }

    private void Update()
    {
        // TODO: Remove, debugging only!
        /*if(Input.GetKeyDown(KeyCode.H))
        {
            //int randomIndex = Random.Range(0, levelManager.LevelTiles.Count);
            //Vector2 spawnPos = levelManager.LevelTiles.Values.ElementAt(randomIndex).TilePosition;
            SpawnDecal("blood", levelManager.GetEntity<Player>().CenterOfMass);
        }*/
    }

    public static void SpawnDecal(string decalID, Vector2 worldPos)
    {
        if(decalDatabase.TryGetValue(decalID, out DecalData decalData) && decalData.sprites != null && decalData.sprites.Count > 0)
        {
            for(int i = 0; i < decalData.spawnIterations; i++)
            {
                int randomIndex = Random.Range(0, decalData.sprites.Count);
                Sprite randomSprite = decalData.sprites[randomIndex];
                Texture2D decalTexture = ExtractTextureFromSprite(randomSprite);

                // Generate a random scale factor between min and max scale
                float randomScale = Random.Range(decalData.minScale, decalData.maxScale);

                foreach(var levelTile in levelManager.LevelTiles.Values)
                {
                    if(IsTileInRange(levelTile, worldPos, decalData))
                    {
                        if(Random.value > decalData.spawnChance || !decalData.tilesToAffect.Contains(levelTile.TileData.id))
                        {
                            continue;
                        }

                        float randomRotation = Random.Range(0f, 360f);
                        Vector2 offsetPosition = ApplyRandomOffset(worldPos, decalData.maxSpawnOffset);

                        BlendTextures(levelTile, decalTexture, offsetPosition, randomRotation, randomScale);
                    }
                }
            }
        }
    }


    private static bool IsTileInRange(LevelTile levelTile, Vector2 decalPosition, DecalData decalData)
    {
        return Vector2.Distance(decalPosition, levelTile.TilePosition) <= decalData.spawnRange;
    }

    private static void BlendTextures(LevelTile levelTile, Texture2D decalTexture, Vector2 decalPosition, float rotationAngle, float scale)
    {
        SpriteRenderer tileRenderer = levelTile.TileGFX;
        int width = (int)tileRenderer.sprite.textureRect.width;
        int height = (int)tileRenderer.sprite.textureRect.height;
        Texture2D tileTexture = new Texture2D(width, height);
        Rect spriteRect = tileRenderer.sprite.textureRect;
        Color[] tilePixels = tileRenderer.sprite.texture.GetPixels((int)spriteRect.x, (int)spriteRect.y, (int)spriteRect.width, (int)spriteRect.height);
        tileTexture.SetPixels(tilePixels);

        Texture2D scaledDecalTexture = ScaleTexture(decalTexture, scale);
        Texture2D rotatedDecalTexture = RotateTexture(scaledDecalTexture, rotationAngle);

        int decalWidth = rotatedDecalTexture.width;
        int decalHeight = rotatedDecalTexture.height;
        Vector2 tilePosition = levelTile.TilePosition;

        int startX = Mathf.Max(0, Mathf.FloorToInt(decalPosition.x - tilePosition.x));
        int startY = Mathf.Max(0, Mathf.FloorToInt(decalPosition.y - tilePosition.y));
        int endX = Mathf.Min(tileTexture.width, startX + decalWidth);
        int endY = Mathf.Min(tileTexture.height, startY + decalHeight);

        for(int y = startY; y < endY; y++)
        {
            for(int x = startX; x < endX; x++)
            {
                int decalX = x - startX;
                int decalY = y - startY;

                Color decalColor = rotatedDecalTexture.GetPixel(decalX, decalY);

                if(decalColor.a > 0)
                {
                    tileTexture.SetPixel(x, y, decalColor);
                }
            }
        }

        tileTexture.Apply();

        tileTexture.filterMode = FilterMode.Point;
        tileTexture.anisoLevel = 0;
        tileTexture.requestedMipmapLevel = 0;
        Rect rect = new Rect(0, 0, tileTexture.width, tileTexture.height);
        Vector2 pivot = new Vector2(0.5f, 0.5f);
        float pixelsPerUnit = levelTile.TileGFX.sprite.pixelsPerUnit;
        levelTile.TileGFX.sprite = Sprite.Create(tileTexture, rect, pivot, pixelsPerUnit);
    }

    private static Vector2 ApplyRandomOffset(Vector2 position, float maxOffset)
    {
        float offsetX = Random.Range(-maxOffset, maxOffset);
        float offsetY = Random.Range(-maxOffset, maxOffset);
        return position + new Vector2(offsetX, offsetY);
    }

    private static Texture2D RotateTexture(Texture2D original, float angle)
    {
        Texture2D rotated = new Texture2D(original.width, original.height);
        Color[] pixels = original.GetPixels();
        float radians = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        Vector2 center = new Vector2(original.width / 2, original.height / 2);

        for(int y = 0; y < original.height; y++)
        {
            for(int x = 0; x < original.width; x++)
            {
                // Calculate rotated position...
                Vector2 uv = new Vector2(x - center.x, y - center.y);
                float newX = uv.x * cos - uv.y * sin + center.x;
                float newY = uv.x * sin + uv.y * cos + center.y;

                // Ensure the new position is within bounds...
                if(newX >= 0 && newX < original.width && newY >= 0 && newY < original.height)
                {
                    // Map the rotated position to the corresponding pixel...
                    int pixelX = Mathf.FloorToInt(newX);
                    int pixelY = Mathf.FloorToInt(newY);

                    // Set the pixel in the rotated texture...
                    rotated.SetPixel(x, y, pixels[pixelY * original.width + pixelX]);
                }
                else
                {
                    // If out of bounds, set the pixel to transparent...
                    rotated.SetPixel(x, y, Color.clear);
                }
            }
        }

        rotated.Apply();
        return rotated;
    }

    private static Texture2D ScaleTexture(Texture2D original, float scale)
    {
        int newWidth = Mathf.FloorToInt(original.width * scale);
        int newHeight = Mathf.FloorToInt(original.height * scale);

        Texture2D scaled = new Texture2D(newWidth, newHeight);
        Color[] originalPixels = original.GetPixels();
        Color[] scaledPixels = new Color[newWidth * newHeight];

        for(int y = 0; y < newHeight; y++)
        {
            for(int x = 0; x < newWidth; x++)
            {
                float srcX = x / scale;
                float srcY = y / scale;

                int px = Mathf.FloorToInt(srcX);
                int py = Mathf.FloorToInt(srcY);

                if(px >= 0 && px < original.width && py >= 0 && py < original.height)
                {
                    scaledPixels[y * newWidth + x] = originalPixels[py * original.width + px];
                }
                else
                {
                    scaledPixels[y * newWidth + x] = Color.clear;
                }
            }
        }

        scaled.SetPixels(scaledPixels);
        scaled.Apply();

        return scaled;
    }

    private static Texture2D ExtractTextureFromSprite(Sprite sprite)
    {
        if(sprite == null) return null;

        Texture2D sourceTexture = sprite.texture;
        Rect spriteRect = sprite.textureRect;

        Texture2D extractedTexture = new Texture2D((int)spriteRect.width, (int)spriteRect.height);
        Color[] pixels = sourceTexture.GetPixels((int)spriteRect.x, (int)spriteRect.y, (int)spriteRect.width, (int)spriteRect.height);
        extractedTexture.SetPixels(pixels);
        extractedTexture.Apply();

        return extractedTexture;
    }
}
