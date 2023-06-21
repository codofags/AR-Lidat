using UnityEngine;

public class ModelInteraction : MonoBehaviour
{
    private bool isRotating;                  // Флаг вращения
    private float rotationSpeed = 1f;         // Скорость вращения

    private bool isScaling;                   // Флаг масштабирования
    private float scaleSpeed = 0.5f;          // Скорость масштабирования

    private Vector2 lastTouchPosition;        // Последняя позиция касания

    void Update()
    {
        HandleTouchInput();
    }

    void HandleTouchInput()
    {
        if (Input.touchCount == 1)
        {
            // Одно касание - вращение
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                isRotating = true;
                lastTouchPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Moved && isRotating)
            {
                Vector2 delta = touch.position - lastTouchPosition;
                transform.Rotate(Vector3.up, -delta.x * rotationSpeed, Space.World);
                lastTouchPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                isRotating = false;
            }
        }
        else if (Input.touchCount == 2)
        {
            // Два касания - масштабирование
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            if (touch2.phase == TouchPhase.Began)
            {
                isScaling = true;
            }
            else if (touch1.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Moved && isScaling)
            {
                Vector2 delta1 = touch1.deltaPosition;
                Vector2 delta2 = touch2.deltaPosition;
                float touchDeltaMag = (delta1 - delta2).magnitude;
                float scaleAmount = touchDeltaMag * scaleSpeed;

                transform.localScale += new Vector3(scaleAmount, scaleAmount, scaleAmount);
            }
            else if (touch1.phase == TouchPhase.Ended || touch2.phase == TouchPhase.Ended)
            {
                isScaling = false;
            }
        }
    }
}
