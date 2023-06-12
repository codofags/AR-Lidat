using UnityEngine;

public static class Texture2DExtensions
{
    public static Texture2D CropTexture(this Texture2D texture, Rect rect)
    {
        int x = Mathf.FloorToInt(rect.x);
        int y = Mathf.FloorToInt(rect.y);
        int width = Mathf.FloorToInt(rect.width);
        int height = Mathf.FloorToInt(rect.height);

        Color[] pixels = texture.GetPixels(x, y, width, height);
        Texture2D croppedTexture = new Texture2D(width, height);
        croppedTexture.SetPixels(pixels);
        croppedTexture.Apply();

        return croppedTexture;
    }
}