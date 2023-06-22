using UnityEngine;

internal class MeshData
{
    public MeshFilter MeshFilter;
    public Texture2D Texture;

    public MeshData(MeshFilter meshFilter, Texture2D texture)
    {
        this.MeshFilter = meshFilter;
        this.Texture = texture;
    }
}
