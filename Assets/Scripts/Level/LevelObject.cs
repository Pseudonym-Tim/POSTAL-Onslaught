using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles everything related to level objects...
/// </summary>
public class LevelObject : MonoBehaviour
{
    [SerializeField] private List<Sprite> levelObjectSprites;
    private SpriteRenderer levelObjectGFX;

    private void Awake()
    {
        RandomizeGFX();
        // TODO: Pull navigation related settings from json data?
    }

    private void RandomizeGFX()
    {
        levelObjectGFX = GetComponentInChildren<SpriteRenderer>();
        int randomIndex = Random.Range(0, levelObjectSprites.Count);
        levelObjectGFX.sprite = levelObjectSprites[randomIndex];
    }

    public ObjectData ObjectData { get; set; } = null;
}
