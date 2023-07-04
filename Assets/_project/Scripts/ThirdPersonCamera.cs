using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float distance = 5f;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float minVerticalAngle = -80f;
    [SerializeField] private float maxVerticalAngle = 80f;
    [SerializeField] private float rotationSpeed = 3f;
    [SerializeField] private float damping = 5f;
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float zoomSpeed = 5f; // Скорость масштабирования

    public float MoveSpeed { get => movementSpeed; set => movementSpeed = value; }
    public bool IsInteractable = false;
    private Vector3 offset;

    private float currentX = 0f;
    private float currentY = 0f;

    private bool isMoving = false;
    private Vector2 initialTouchPosition;
    private float initialDistance; // Изначальное расстояние между пальцами

    private void Start()
    {
        offset = transform.position - target.position;
    }

    private void LateUpdate()
    {
        if (!IsInteractable) return;

        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Moved)
            {
                currentX += touch.deltaPosition.x * rotationSpeed;
                currentY -= touch.deltaPosition.y * rotationSpeed;

                currentY = Mathf.Clamp(currentY, minVerticalAngle, maxVerticalAngle);
            }
        }
        else if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            if (touch1.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Moved)
            {
                if (!isMoving)
                {
                    initialTouchPosition = (touch1.position + touch2.position) / 2f;
                    initialDistance = Vector2.Distance(touch1.position, touch2.position);
                    isMoving = true;
                }
                else
                {
                    Vector2 currentTouchPosition = (touch1.position + touch2.position) / 2f;
                    Vector2 touchDelta = currentTouchPosition - initialTouchPosition;

                    Vector3 moveDirection = Quaternion.Euler(0f, currentX, 0f) * new Vector3(touchDelta.x, 0f, touchDelta.y) * movementSpeed * Time.deltaTime;

                    // Исправление направления движения вверх и вниз относительно поворота камеры
                    Vector3 verticalOffset = Quaternion.Euler(currentY, currentX, 0f) * Vector3.up * touchDelta.y * movementSpeed * Time.deltaTime;

                    target.position += verticalOffset;
                    transform.position += verticalOffset;

                    target.position += moveDirection;
                    transform.position += moveDirection;

                    initialTouchPosition = currentTouchPosition;

                    // Масштабирование при отдалении и сближении пальцев
                    float currentDistance = Vector2.Distance(touch1.position, touch2.position);
                    float zoomDelta = currentDistance - initialDistance;
                    float zoomAmount = zoomDelta * zoomSpeed * Time.deltaTime;
                    distance = Mathf.Clamp(distance - zoomAmount, minDistance, maxDistance);
                }
            }
            else
            {
                isMoving = false;
            }
        }

        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0f);
        Vector3 targetPosition = target.position - rotation * Vector3.forward * distance;
        transform.position = Vector3.Lerp(transform.position, targetPosition, damping * Time.deltaTime);
        transform.rotation = rotation;
    }
}
