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
        /*if(Input.GetKeyDown(KeyCode.H) && Application.isEditor)
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

                        int randomRotation = Random.Range(0, 4) * 90;
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

    private static void BlendTextures(LevelTile levelTile, Texture2D decalTexture, Vector2 decalPosition, int rotationAngle, float scale)
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

    private static Texture2D RotateTexture(Texture2D original, int angle)
    {
        // Ensure the angle is a multiple of 90 degrees
        angle = Mathf.RoundToInt(angle / 90f) * 90;

        int width = original.width;
        int height = original.height;
        Texture2D rotated = new Texture2D(width, height);

        Color[] originalPixels = original.GetPixels();
        Color[] rotatedPixels = new Color[originalPixels.Length];

        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                int newX = x, newY = y;
                switch(angle)
                {
                    case 90:
                        newX = y;
                        newY = width - 1 - x;
                        break;
                    case 180:
                        newX = width - 1 - x;
                        newY = height - 1 - y;
                        break;
                    case 270:
                        newX = height - 1 - y;
                        newY = x;
                        break;
                    default: // 0 degrees
                        newX = x;
                        newY = y;
                        break;
                }

                // Ensure the new indices are within the bounds of the array
                if(newX >= 0 && newX < width && newY >= 0 && newY < height)
                {
                    rotatedPixels[newY * width + newX] = originalPixels[y * width + x];
                }
            }
        }

        rotated.SetPixels(rotatedPixels);
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
