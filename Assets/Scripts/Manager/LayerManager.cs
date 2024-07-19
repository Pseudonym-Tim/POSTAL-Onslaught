using UnityEngine;

/// <summary>
/// More conveniant layer management...
/// </summary>
public static class LayerManager
{
    public static class Layers
    {
        public static int DEFAULT = 0;
        public static int PLAYER = 3;
        public static int NPC = 7;
        public static int ENEMY = 6;
    }

    public static class SortingLayers
    {
        public static int DEFAULT = SortingLayer.NameToID("Default");
    }

    public static class Masks
    {
        public static LayerMask DEFAULT = 1 << Layers.DEFAULT;
        public static LayerMask PLAYER = 1 << Layers.PLAYER;
        public static LayerMask ENEMY = 1 << Layers.ENEMY;
        public static LayerMask NPC = 1 << Layers.NPC | ENEMY;
        public static LayerMask NAVIGABLE = DEFAULT;
        public static LayerMask SHOOTABLE_PLAYER = DEFAULT | PLAYER;
        public static LayerMask SHOOTABLE_NPC = DEFAULT | NPC;
    }

    public static int ToLayerID(LayerMask layerMask)
    {
        return Mathf.RoundToInt(Mathf.Log(layerMask.value, 2));
    }
}
