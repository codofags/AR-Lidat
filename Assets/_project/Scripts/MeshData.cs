using System;
using UnityEngine;

[Serializable]
public class MeshData
{
    public MeshFilter MeshFilter;
    public Texture2D Texture;

    public MeshData()
    {

    }

    public MeshData(MeshFilter meshFilter, Texture2D texture)
    {
        this.MeshFilter = meshFilter;
        this.Texture = texture;
    }
}
