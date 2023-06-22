using Unity.Collections;
using UnityEngine;

public static class Texture2DExtensions
{
    public static void RotateTexture(this Texture2D texture, bool clockwise)
    {
        Color32[] original = texture.GetPixels32();
        Color32[] rotated = new Color32[original.Length];
        int w = texture.width;
        int h = texture.height;

        int iRotated, iOriginal;

        for (int j = 0; j < h; ++j)
        {
            for (int i = 0; i < w; ++i)
            {
                iRotated = (i + 1) * h - j - 1;
                iOriginal = clockwise ? original.Length - 1 - (j * w + i) : j * w + i;
                rotated[iRotated] = original[iOriginal];
            }
        }

        Texture2D rotatedTexture = new Texture2D(h, w);
        rotatedTexture.SetPixels32(rotated);
        rotatedTexture.Apply();
        texture = rotatedTexture;
    }

    public static void FlipTexture(this Texture2D texture)
    {
        Color32[] original = texture.GetPixels32();
        Color32[] flipped = new Color32[original.Length];
        int w = texture.width;
        int h = texture.height;

        int iFlipped, iOriginal;

        for (int j = 0; j < h; ++j)
        {
            for (int i = 0; i < w; ++i)
            {
                iFlipped = j * w + (w - i - 1);
                iOriginal = j * w + i;
                flipped[iFlipped] = original[iOriginal];
            }
        }

        Texture2D flippedTexture = new Texture2D(w, h);
        flippedTexture.SetPixels32(flipped);
        flippedTexture.Apply();
        texture = flippedTexture;
    }
}