using UnityEngine;

public class ResizePlane : MonoBehaviour
{
    private Vector3 initialScale;
                  public GameObject referenceObject;
    void Start()
    {
        initialScale = transform.localScale;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                ///if (hit.collider.gameObject == gameObject)
                {
                    // Создаем объект в точке рейкаста
  
                    referenceObject.transform.position = hit.point;

                    // Вычисляем дистанцию между плоскостью и объектом
                    float distance = Vector3.Distance(transform.position, referenceObject.transform.position);

                    // Ограничиваем масштабирование от 0.1 до 10 (можете настроить)
                    distance = Mathf.Clamp(distance, 0.1f, 10f);

                    // Изменяем масштаб плоскости только по высоте
                    Vector3 newScale = initialScale;
                    newScale.z = distance/10;
                    transform.localScale = newScale;
Debug.Log(distance );
                    // Удаляем временный объект
                    //Destroy(referenceObject);
                }
            }
        }
    }
}