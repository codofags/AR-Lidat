using UnityEngine;

public class ModelInteraction : MonoBehaviour
{
    [SerializeField] private Camera _arCamera;
    [SerializeField] private float _zoomSpeed = 1f;
    [SerializeField] private float _minFOV = 10f;
    [SerializeField] private float _maxFOV = 90f;

    private bool _isRotating;
    private float _rotationSpeed = 1f;

    private bool _isMoving;
    private float _moveSpeed = 0.1f;

    private Vector2 _lastTouchPosition;
    private Vector3 _initialObjectPosition;


    private float initialFOV;

    void Start()
    {
        _initialObjectPosition = transform.position;
        initialFOV = _arCamera.fieldOfView;
    }

    void Update()
    {
        HandleTouchInput();
    }

    public void SetDefault()
    {
        transform.position = _initialObjectPosition;
        _arCamera.fieldOfView = initialFOV;
    }

    void HandleTouchInput()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                _isRotating = true;
                _lastTouchPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Moved && _isRotating)
            {
                Vector2 delta = touch.position - _lastTouchPosition;
                transform.Rotate(Vector3.up, -delta.x * _rotationSpeed, Space.World);
                _lastTouchPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                _isRotating = false;
            }
        }
        else if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            if (touch2.phase == TouchPhase.Began)
            {
                _isMoving = true;
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
                    // Изменение поля зрения камеры (FOV) на основе изменения расстояния
                    float zoomAmount = deltaDistance * _zoomSpeed;
                    float newFOV = _arCamera.fieldOfView - zoomAmount;
                    _arCamera.fieldOfView = Mathf.Clamp(newFOV, _minFOV, _maxFOV);
                }

                // Перемещение
                Vector2 centerPosition = (touch1.position + touch2.position) / 2;
                Vector2 lastCenterPosition = (prevTouch1Pos + prevTouch2Pos) / 2;

                Vector3 moveDelta = (centerPosition - lastCenterPosition) * _moveSpeed;
                transform.position += new Vector3(moveDelta.x, 0, moveDelta.y);
            }
            else if (touch1.phase == TouchPhase.Ended || touch2.phase == TouchPhase.Ended)
            {
                _isMoving = false;
            }
        }
    }
}
