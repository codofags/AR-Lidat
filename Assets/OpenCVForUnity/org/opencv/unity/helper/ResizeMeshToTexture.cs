using UnityEngine;
public class ResizeMeshToTexture : MonoBehaviour
{
    public MeshFilter meshFilter; // Компонент MeshFilter меша
    public MeshRenderer meshRenderer; // Компонент MeshRenderer меша
    public Transform sourceMeshTransform;
    public void Re()
    {
       if (sourceMeshTransform == null)
        {
            Debug.LogError("Assign the source mesh transform!");
            return;
        }

        // Копируем масштаб из исходного меша
        Vector3 scale = sourceMeshTransform.localScale;
        transform.localScale = scale;

        Debug.Log("Mesh scaled to match another mesh's scale!");
    }
}