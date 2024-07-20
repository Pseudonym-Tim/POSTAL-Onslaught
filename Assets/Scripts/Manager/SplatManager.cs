using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Handles everything related to splatting level tiles...
/// </summary>
public class SplatManager : Singleton<SplatManager>
{
    public List<SplatData> splatterList;
    private static LevelManager levelManager;
    private static Dictionary<string, SplatData> splatDatabase;

    public void Setup()
    {
        levelManager = FindFirstObjectByType<LevelManager>();
        splatDatabase = new Dictionary<string, SplatData>();
        splatDatabase = splatterList.ToDictionary(splat => splat.splatID);
    }

    private void Update()
    {
        // TODO: Remove, debugging only!
        /*if(Input.GetKeyDown(KeyCode.H))
        {
            //int randomIndex = Random.Range(0, levelManager.LevelTiles.Count);
            //Vector2 spawnPos = levelManager.LevelTiles.Values.ElementAt(randomIndex).TilePosition;
            SpawnSplatter("blood_splat", levelManager.GetEntity<Player>().CenterOfMass);
        }*/
    }

    public static void SpawnSplatter(string splatID, Vector2 worldPos)
    {
        if(splatDatabase.TryGetValue(splatID, out SplatData splatData) && splatData.sprites != null && splatData.sprites.Count > 0)
        {
            for(int i = 0; i < splatData.spawnIterations; i++)
            {
                int randomIndex = Random.Range(0, splatData.sprites.Count);
                Sprite randomSprite = splatData.sprites[randomIndex];
                Texture2D splatTexture = ExtractTextureFromSprite(randomSprite);

                // Generate a random scale factor between min and max scale
                float randomScale = Random.Range(splatData.minScale, splatData.maxScale);

                foreach(var levelTile in levelManager.LevelTiles.Values)
                {
                    if(IsTileInSplatRange(levelTile, worldPos, splatData))
                    {
                        if(Random.value > splatData.spawnChance || !splatData.tilesToAffect.Contains(levelTile.TileData.id))
                        {
                            continue;
                        }

                        float randomRotation = Random.Range(0f, 360f);
                        Vector2 offsetSplatPosition = ApplyRandomOffset(worldPos, splatData.maxSpawnOffset);

                        BlendTextures(levelTile, splatTexture, offsetSplatPosition, randomRotation, randomScale);
                    }
                }
            }
        }
    }


    private static bool IsTileInSplatRange(LevelTile levelTile, Vector2 splatPosition, SplatData splatData)
    {
        return Vector2.Distance(splatPosition, levelTile.TilePosition) <= splatData.spawnRange;
    }

    private static void BlendTextures(LevelTile levelTile, Texture2D splatTexture, Vector2 splatPosition, float rotationAngle, float scale)
    {
        SpriteRenderer tileRenderer = levelTile.TileGFX;
        int width = (int)tileRenderer.sprite.textureRect.width;
        int height = (int)tileRenderer.sprite.textureRect.height;
        Texture2D tileTexture = new Texture2D(width, height);
        Rect spriteRect = tileRenderer.sprite.textureRect;
        Color[] tilePixels = tileRenderer.sprite.texture.GetPixels((int)spriteRect.x, (int)spriteRect.y, (int)spriteRect.width, (int)spriteRect.height);
        tileTexture.SetPixels(tilePixels);

        Texture2D scaledSplatTexture = ScaleTexture(splatTexture, scale);
        Texture2D rotatedSplatTexture = RotateTexture(scaledSplatTexture, rotationAngle);

        int splatWidth = rotatedSplatTexture.width;
        int splatHeight = rotatedSplatTexture.height;
        Vector2 tilePosition = levelTile.TilePosition;

        int startX = Mathf.Max(0, Mathf.FloorToInt(splatPosition.x - tilePosition.x));
        int startY = Mathf.Max(0, Mathf.FloorToInt(splatPosition.y - tilePosition.y));
        int endX = Mathf.Min(tileTexture.width, startX + splatWidth);
        int endY = Mathf.Min(tileTexture.height, startY + splatHeight);

        for(int y = startY; y < endY; y++)
        {
            for(int x = startX; x < endX; x++)
            {
                int splatX = x - startX;
                int splatY = y - startY;

                Color splatColor = rotatedSplatTexture.GetPixel(splatX, splatY);

                if(splatColor.a > 0)
                {
                    tileTexture.SetPixel(x, y, splatColor);
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
