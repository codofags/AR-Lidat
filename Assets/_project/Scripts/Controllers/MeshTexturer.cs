using UnityEngine;

public class MeshTexturer : MonoBehaviour
{
    [SerializeField] Texture _texture;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryColorize();
        }
    }

    [ContextMenu("Colorize")]
    public async void TryColorize()
    {
        gameObject.GetComponent<MeshFilter>().TextureMesh(_texture);
    }

}
