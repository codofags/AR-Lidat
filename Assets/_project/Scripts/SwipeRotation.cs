using UnityEngine;
using UnityEngine.AI;

public class SwipeRotation : MonoBehaviour
{
    private Vector2 startSwipePos;
    private bool isSwiping = false;
    public float rotationSpeed = 5f;

    private void Update()
    {
        CheckTouch();

        // Поворот модели при свайпе
        if (isSwiping)
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                Vector2 currentSwipePos = touch.position;

                float swipeDelta = currentSwipePos.x - startSwipePos.x;

                // Поворот модели вокруг оси Y
                transform.Rotate(Vector3.up, swipeDelta * rotationSpeed * Time.deltaTime);
            }
        }
    }

    private void CheckTouch()
    {
        // Определение начала свайпа
        if (Input.GetMouseButton(0))
        {
            Touch touch = Input.GetTouch(0);

            isSwiping = true;
            if (touch.phase == TouchPhase.Began)
            {
            }
            // Определение окончания свайпа
            else if (touch.phase == TouchPhase.Ended)
            {
                isSwiping = false;
            }
        }
    }
}