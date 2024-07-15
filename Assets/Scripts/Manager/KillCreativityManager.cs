using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles everything related to player weapon creativity...
/// </summary>
public class KillCreativityManager : Singleton<KillCreativityManager>
{
    public const int MAX_CREATIVITY = 10;
    private HashSet<string> uniqueWeaponsUsed;
    private int totalWeaponCount = 0;

    public void Setup()
    {
        uniqueWeaponsUsed = new HashSet<string>();
        EntityData weaponPickupEntityData = EntityManager.RegisteredEntities["weapon_pickup"];
        JArray weaponPool = (JArray)weaponPickupEntityData.jsonData["weaponPool"];
        totalWeaponCount = weaponPool.Count;
    }

    public void RegisterWeaponUse(string weaponName)
    {
        if(!uniqueWeaponsUsed.Contains(weaponName))
        {
            uniqueWeaponsUsed.Add(weaponName);
            LevelManager.LevelStats.UniqueWeaponsUsed++;
        }
    }

    public int CalculateCreativityRating()
    {
        int uniqueWeaponCount = uniqueWeaponsUsed.Count;
        float creativityRating = (float)uniqueWeaponCount / totalWeaponCount * (float)MAX_CREATIVITY;
        return Mathf.Clamp(Mathf.RoundToInt(creativityRating), 0, MAX_CREATIVITY);
    }
}
