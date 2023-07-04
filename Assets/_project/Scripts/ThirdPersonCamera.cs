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
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float movementSpeed = 5f;

    public float MoveSpeed { get => movementSpeed; set => movementSpeed = value; }
    public float ZoomSpeed { get => zoomSpeed; set => zoomSpeed = value; }
    public bool IsInteractable = false;

    private Vector3 offset;

    private float currentX = 0f;
    private float currentY = 0f;

    private Vector2 lastTouchPosition;

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

            if (touch2.phase == TouchPhase.Moved)
            {
                float prevDistance = Vector2.Distance(touch1.position - touch1.deltaPosition, touch2.position - touch2.deltaPosition);
                float currentDistance = Vector2.Distance(touch1.position, touch2.position);

                float deltaDistance = currentDistance - prevDistance;
                float zoomAmount = deltaDistance * zoomSpeed;

                distance -= zoomAmount;
                distance = Mathf.Clamp(distance, minDistance, maxDistance);
            }

            if (touch1.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Moved)
            {
                Vector2 touchDelta = touch1.deltaPosition - touch2.deltaPosition;

                Vector3 moveDirection = new Vector3(touchDelta.x, 0f, touchDelta.y) * movementSpeed * Time.deltaTime;
                moveDirection = Quaternion.Euler(0f, currentX, 0f) * moveDirection;

                target.position += moveDirection;
                transform.position += moveDirection;
            }
        }

        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0f);
        Vector3 targetPosition = target.position - rotation * Vector3.forward * distance;
        transform.position = Vector3.Lerp(transform.position, targetPosition, damping * Time.deltaTime);
        transform.rotation = rotation;
    }
}
