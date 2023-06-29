using UnityEngine;

public class ModelInteraction : MonoBehaviour
{
    [SerializeField] private Camera _camera;

    private bool isRotating;
    private float rotationSpeed = 1f;

    private bool isScaling;
    private float scaleSpeed = 0.1f;

    private bool isMoving;
    private float moveSpeed = 0.1f;

    private Vector2 lastTouchPosition;
    private Vector3 initialObjectPosition;

    void Start()
    {
        initialObjectPosition = transform.position;
    }

    void Update()
    {
        HandleTouchInput();
    }

    void HandleTouchInput()
    {
        if (Input.touchCount == 1)
        {
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
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            if (touch2.phase == TouchPhase.Began)
            {
                isScaling = true;
            }
            else if (touch1.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Moved)
            {
                // Определение расстояния между пальцами на предыдущем кадре
                Vector2 prevTouch1Pos = touch1.position - touch1.deltaPosition;
                Vector2 prevTouch2Pos = touch2.position - touch2.deltaPosition;
                float prevDistance = Vector2.Distance(prevTouch1Pos, prevTouch2Pos);

                // Определение расстояния между пальцами на текущем кадре
                float touchDeltaMag = (touch1.position - touch2.position).magnitude;

                // Определение изменения расстояния между пальцами
                float deltaDistance = touchDeltaMag - prevDistance;

                if (Mathf.Abs(deltaDistance) >= 0.1f)
                {
                    //// Изменение масштаба на основе изменения расстояния
                    float scaleAmount = deltaDistance * scaleSpeed;

                    //var newScale = new Vector3(scaleAmount, scaleAmount, scaleAmount);

                    //if (transform.localScale.x <= 0f && newScale.x <= 0f)
                    //    transform.localScale = Vector3.one * 0.01f;
                    //else
                    //    transform.localScale += newScale;

                    var newFov = _camera.fieldOfView + scaleAmount;
                    newFov = Mathf.Clamp(newFov, 10f, 120f);
                    _camera.fieldOfView = newFov;

                }
                // Перемещение
                isMoving = true;
                Vector2 centerPosition = (touch1.position + touch2.position) / 2;
                Vector2 lastCenterPosition = (prevTouch1Pos + prevTouch2Pos) / 2;

                Vector3 moveDelta = (centerPosition - lastCenterPosition) * moveSpeed;
                transform.position += new Vector3(moveDelta.x, 0, moveDelta.y);
            }
            else if (touch1.phase == TouchPhase.Ended || touch2.phase == TouchPhase.Ended)
            {
                isScaling = false;
                isMoving = false;
            }
        }
    }
}
