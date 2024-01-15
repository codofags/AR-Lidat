using UnityEngine;

public class MatchTransform : MonoBehaviour
{
    public string targetObjectName; // Укажите имя объекта, к которому вы хотите подстроить Quad

    void Update()
    {
        if (!string.IsNullOrEmpty(targetObjectName))
        {
            // Ищем объект по имени
            GameObject targetObject = GameObject.Find(targetObjectName);

            if (targetObject != null)
            {
           
                 Renderer targetRenderer = targetObject.GetComponent<Renderer>();
                 Renderer thisO=this.gameObject.GetComponent<Renderer>();
                 thisO.material = new Material(targetRenderer.sharedMaterial);
    
                // Копируем масштаб, позицию и поворот из targetObject к текущему объекту (Quad)
                 targetObject.transform.localScale=transform.localScale;
                 targetObject.transform.position=transform.position;
                 targetObject.transform.rotation=transform.rotation;
            }
            else
            {
                Debug.LogWarning("Object with the name " + targetObjectName + " not found.");
            }
        }
        else
        {
            Debug.LogWarning("TargetObjectName is not set. Please provide the name of the target object.");
        }
    }
}