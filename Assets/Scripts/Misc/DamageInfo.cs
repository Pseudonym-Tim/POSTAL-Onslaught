using UnityEngine;

[System.Serializable]
public class DamageInfo
{
    public int damageAmount = 0;
    public Vector2 damageOrigin;
    public Entity attackerEntity = null;
}