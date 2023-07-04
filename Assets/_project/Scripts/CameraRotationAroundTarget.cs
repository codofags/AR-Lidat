using UnityEngine;

public class MobileCameraControl : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private float zoomSpeed = 1f;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float verticalRotationLimit = 80f;

    private bool isRotating;
    private bool isVerticalRotating;
    private bool isMoving;

    private Vector2 touchStartPos;
    private float rotationY = 0f;
    private float distance;

    void Start()
    {
        distance = Vector3.Distance(transform.position, target.position);
    }

    void Update()
    {
        HandleTouchInput();
        HandleMouseInput();
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                touchStartPos = touch.position;
                isRotating = true;
                isVerticalRotating = false;
                isMoving = false;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                Vector2 touchDelta = touch.position - touchStartPos;

                if (!isRotating && Mathf.Abs(touchDelta.x) > Mathf.Abs(touchDelta.y))
                {
                    isRotating = true;
                    isVerticalRotating = false;
                    isMoving = false;
                }
                else if (!isVerticalRotating && Mathf.Abs(touchDelta.y) > Mathf.Abs(touchDelta.x))
                {
                    isVerticalRotating = true;
                    isRotating = false;
                    isMoving = false;
                }
                else if (!isMoving)
                {
                    isMoving = true;
                    isRotating = false;
                    isVerticalRotating = false;
                }

                if (isRotating)
                {
                    float mouseX = touchDelta.x * rotationSpeed * Time.deltaTime;
                    float mouseY = touchDelta.y * rotationSpeed * Time.deltaTime;
                    transform.RotateAround(target.position, Vector3.up, mouseX);
                    transform.RotateAround(target.position, Vector3.right, mouseY);
                    Debug.Log($"Rotation: {transform.rotation}");
                }

                if (isMoving)
                {
                    Vector3 moveDelta = new Vector3(touchDelta.x, 0f, touchDelta.y) * rotationSpeed * Time.deltaTime;
                    Vector3 moveVector = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f) * moveDelta;
                    target.position += moveVector;
                }

                touchStartPos = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                isRotating = false;
                isVerticalRotating = false;
                isMoving = false;
            }
        }
        else if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            if (touch2.phase == TouchPhase.Began)
            {
                distance = Vector3.Distance(transform.position, target.position);
            }
            else if (touch1.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Moved)
            {
                Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
                Vector2 touch2PrevPos = touch2.position - touch2.deltaPosition;

                float prevMagnitude = (touch1PrevPos - touch2PrevPos).magnitude;
                float currentMagnitude = (touch1.position - touch2.position).magnitude;

                float deltaMagnitude = prevMagnitude - currentMagnitude;

                float newDistance = distance + deltaMagnitude * zoomSpeed * Time.deltaTime;
                newDistance = Mathf.Clamp(newDistance, minDistance, maxDistance);

                Vector3 dir = (transform.position - target.position).normalized;
                transform.position = target.position + dir * newDistance;
            }
        }
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            touchStartPos = Input.mousePosition;
            isRotating = true;
            isVerticalRotating = false;
            isMoving = false;
        }
        else if (Input.GetMouseButton(0))
        {
            Vector2 mouseDelta = (Vector2)Input.mousePosition - touchStartPos;

            if (!isRotating && Mathf.Abs(mouseDelta.x) > Mathf.Abs(mouseDelta.y))
            {
                isRotating = true;
                isVerticalRotating = false;
                isMoving = false;
            }
            else if (!isVerticalRotating && Mathf.Abs(mouseDelta.y) > Mathf.Abs(mouseDelta.x))
            {
                isVerticalRotating = true;
                isRotating = false;
                isMoving = false;
            }
            else if (!isMoving)
            {
                isMoving = true;
                isRotating = false;
                isVerticalRotating = false;
            }

            if (isRotating)
            {
                float mouseX = mouseDelta.x * rotationSpeed * Time.deltaTime;
                transform.RotateAround(target.position, Vector3.up, mouseX);
            }

            if (isVerticalRotating)
            {
                float mouseY = mouseDelta.y * rotationSpeed * Time.deltaTime;
                rotationY += mouseY;
                rotationY = Mathf.Clamp(rotationY, -verticalRotationLimit, verticalRotationLimit);

                Vector3 rotation = transform.localEulerAngles;
                rotation.x = -rotationY;
                transform.localEulerAngles = rotation;
            }

            if (isMoving)
            {
                Vector3 moveDelta = new Vector3(mouseDelta.x, 0f, mouseDelta.y) * rotationSpeed * Time.deltaTime;
                Vector3 moveVector = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f) * moveDelta;
                target.position += moveVector;
            }

            touchStartPos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isRotating = false;
            isVerticalRotating = false;
            isMoving = false;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        float newDistance = distance + scroll * zoomSpeed * Time.deltaTime;
        newDistance = Mathf.Clamp(newDistance, minDistance, maxDistance);

        Vector3 dir = (transform.position - target.position).normalized;
        transform.position = target.position + dir * newDistance;
    }
}