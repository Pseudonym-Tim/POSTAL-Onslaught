using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helper class for images...
/// </summary>
public static class ImageHelper
{
    public enum PivotAxis
    {
        X,
        Y,
        ALL
    }

    public static void SetNativePivot(Image image, PivotAxis axis = PivotAxis.ALL)
    {
        if(image.sprite == null)
        {
            Debug.LogWarning("Image Sprite is null. Cannot set native pivot...");
            return;
        }

        Vector2 pivotPixel = image.sprite.pivot;
        Rect spriteRect = image.sprite.rect;
        Vector2 pivotNormalized = new Vector2(pivotPixel.x / spriteRect.width, pivotPixel.y / spriteRect.height);

        Vector2 newPivot = image.rectTransform.pivot;

        switch(axis)
        {
            case PivotAxis.X:
                newPivot.x = pivotNormalized.x;
                break;
            case PivotAxis.Y:
                newPivot.y = pivotNormalized.y;
                break;
            case PivotAxis.ALL:
                newPivot = pivotNormalized;
                break;
        }

        image.rectTransform.pivot = newPivot;
    }

    public static void SetNativeSize(Image image, float scaleFactor)
    {
        if(image.sprite == null)
        {
            Debug.LogWarning("Image Sprite is null. Cannot set native size...");
            return;
        }

        Rect spriteRect = image.sprite.rect;
        Vector2 nativeSize = new Vector2(spriteRect.width, spriteRect.height);
        Vector2 scaledSize = nativeSize * scaleFactor;
        image.rectTransform.sizeDelta = scaledSize;
    }
}
