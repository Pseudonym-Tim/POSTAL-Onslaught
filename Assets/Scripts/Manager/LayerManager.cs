using UnityEngine;

/// <summary>
/// More conveniant layer management...
/// </summary>
public static class LayerManager
{
    public static class Layers
    {
        public static int DEFAULT = 0;
        public static int ENEMY = 0;
    }

    public static class SortingLayers
    {
        public static int DEFAULT = SortingLayer.NameToID("Default");
    }

    public static class Masks
    {
        public static LayerMask DEFAULT = 1 << Layers.DEFAULT;
        public static LayerMask ENEMY = 1 << Layers.ENEMY;
        public static LayerMask SHOOTABLE = DEFAULT | ENEMY;
    }
}
