using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public Camera camera; // Reference to your camera
    public int devider; // The divider value for the texture size
    public Vector2 offset; // Offset for UV coordinates

    private MeshFilter meshFilter; // Reference to the dynamically found or created MeshFilter

    private void Start()
    {
        // Initialize the VRTeleportation_TextureGetter
        VRTeleportation_TextureGetter.Instance.Initialize(OnTextureReceived, devider);
    }

    private void OnTextureReceived(Texture2D receivedTexture, int id)
    {
        // Find or create a MeshFilter if it doesn't exist
        meshFilter = FindOrCreateMeshFilter();

        if (meshFilter != null)
        {
            // Call the GenerateUV method with the dynamically found or created MeshFilter
            meshFilter.GenerateUV(camera, receivedTexture, offset);
        }
    }

    private MeshFilter FindOrCreateMeshFilter()
    {
        MeshFilter meshFilter = FindObjectOfType<MeshFilter>();

       return meshFilter;
    }
}