using UnityEngine;

public class ModelInteraction : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    
    private Transform _target;
    public Transform Target { get { return _target; } set { _target = value; } }
    private bool isRotating;
    private float rotationSpeed = 0.1f;

    public float RotationSpeed
    {
        get { return rotationSpeed; }
        set { rotationSpeed = value; }
    }

    private bool isScaling;
    private float scaleSpeed = 0.01f;

    public float ScaleSpeed
    {
        get { return scaleSpeed; }
        set { scaleSpeed = value; }
    }

    private float maxScale = 1;

    public float MaxScale
    {
        get { return maxScale; }
        set { maxScale = value; }
    }

    private float fovScaleSpeed = 0.01f;

    public float FovScaleSpeed
    {
        get { return fovScaleSpeed; }
        set { fovScaleSpeed = value; }
    }

    private bool isMoving;
    private float moveSpeed = 0.001f;

    public float MoveSpeed
    {
        get { return moveSpeed; }
        set { moveSpeed = value; }
    }

    private float minFov = 5f;
    
    public float MinFov
    {
        get { return minFov; }
        set { minFov = value; }
    }

    private float maxFov = 120f;

    public float MaxFov
    {
        get { return maxFov; }
        set { maxFov = value; }
    }

    public Vector3 minScale { get; set; }

    private Vector2 lastTouchPosition;
    private Vector3 initialObjectPosition;

    [HideInInspector] public bool CanRotate = true;
    
    void Start()
    {
        initialObjectPosition = transform.position;
    }

    void Update()
    {
        if (!CanRotate)
            return;

        HandleTouchInput();
    }

    void HandleTouchInput()
    {
        if (Input.touchCount == 1)
        {
            /*
             * Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                lastTouchPosition = touch.position;
                isRotating = true;
            }
            else if (touch.phase == TouchPhase.Moved && isRotating)
            {
                Vector2 delta = touch.position - lastTouchPosition;
                transform.RotateAround(target.position, Vector3.up, -delta.x * rotationSpeed);
                transform.RotateAround(target.position, transform.right, delta.y * rotationSpeed);

                lastTouchPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                isRotating = false;
            }
             * */
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
                transform.Rotate(Vector3.right, delta.y * rotationSpeed, Space.World);

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

                    Vector3 newScale = _target.localScale + Vector3.one * scaleAmount;
                    newScale = Vector3.ClampMagnitude(newScale, maxScale); // Установите максимальный масштаб по вашему выбору
                    newScale = Vector3.Max(newScale, minScale); // Установите минимальный масштаб по вашему выбору
                    _target.localScale = newScale;

                    // Изменение поля зрения (FOV) на основе изменения расстояния
                    float newFov = _camera.fieldOfView - deltaDistance * fovScaleSpeed;
                    newFov = Mathf.Clamp(newFov, minFov, maxFov); // Установите минимальное и максимальное значение поля зрения
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
