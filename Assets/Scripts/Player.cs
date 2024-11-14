using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour
{
    private enum PlayerState
    {
        Playing,
        Walking,
    }

    private float moveSpeed = 4.0f;
    private float sprintSpeed = 6.0f;
    private float cameraSensitivity = 1.5f;
    private float speedChangeRate = 10.0f;
    private float interactionDistance = 2.5f;

    [SerializeField] private LayerMask interactableLayers;
    [SerializeField] private GameObject cinemachineCameraTarget;

    private Vector2 movementInput;
    private Vector2 lookInput;
    private bool isSprinting;
    private bool cursorEnabled;
    private bool cursorLocked = true;
    private bool cursorInputForLook = true;

    private bool isPlaying;
    private bool canMove = true;
    private bool canLook = true;
    private PlayerState playerState;


    private float _cinemachineTargetPitch;
    private float _speed;
    private float _rotationVelocity;
    private PlayerInput _playerInput;
    private CharacterController _controller;
    private GameObject _mainCamera;

    [SerializeField] private GameObject interactObject;
    [SerializeField] private GameObject Chair;

    private void Awake()
    {
        if (_mainCamera == null)
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

        playerState = PlayerState.Walking;
    }

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        _playerInput = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        if (canMove)
            Move();

        InteractWithObject();
    }

    private void LateUpdate()
    {
        if (canLook)
            CameraRotation();
    }

    private void CameraRotation()
    {
        float _threshold = 0.01f;
        if (lookInput.sqrMagnitude >= _threshold)
        {
            float TopClamp = 90.0f;
            float BottomClamp = -90.0f;

            _cinemachineTargetPitch += lookInput.y * cameraSensitivity;
            _rotationVelocity = lookInput.x * cameraSensitivity;
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);
            cinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);
            transform.Rotate(Vector3.up * _rotationVelocity);
        }
    }

    private void Move()
    {
        float targetSpeed = isSprinting ? sprintSpeed : moveSpeed;
        if (movementInput == Vector2.zero)
            targetSpeed = 0.0f;

        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
        float speedOffset = 0.1f;

        if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed, Time.deltaTime * speedChangeRate);
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
            _speed = targetSpeed;

        Vector3 inputDirection = new Vector3(movementInput.x, 0.0f, movementInput.y).normalized;
        if (movementInput != Vector2.zero) 
            inputDirection = transform.right * movementInput.x + transform.forward * movementInput.y;

        _controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + Vector3.zero * Time.deltaTime);
    }

    private void InteractWithObject()
    {
        Ray ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.green);

        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayers))
        {
            if (!interactObject)
            {
                interactObject = hit.collider.gameObject;
                interactObject.GetComponent<IInteractable>().Highlight();
            }
            else if (interactObject != hit.collider.gameObject)
            {
                interactObject.GetComponent<IInteractable>().Unhighlight();
                interactObject = hit.collider.gameObject;
                interactObject.GetComponent<IInteractable>().Highlight();
            }
        }
        else if (interactObject)
        {
            interactObject.GetComponent<IInteractable>().Unhighlight();
            interactObject = null;
        }
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f)
            lfAngle += 360f;
        if (lfAngle > 360f)
            lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    public void OnMove(InputValue value) => movementInput = value.Get<Vector2>();
    public void OnLook(InputValue value) 
    { 
        if (!cursorEnabled) 
            lookInput = value.Get<Vector2>(); 
    }
    public void OnSprint(InputValue value) => isSprinting = value.isPressed;
    public void OnInteract(InputValue value)
    {
        if (interactObject)
        {
            if (playerState == PlayerState.Playing)
                interactObject.GetComponent<IInteractable>().Interact(Chair);
            else if (playerState == PlayerState.Walking)
                interactObject.GetComponent<IInteractable>().Interact(gameObject);
        }
    }

    public void SitOnChair(Vector3 pos)
    {
        _controller.enabled = false;
        transform.position = pos;
        transform.rotation = interactObject.transform.rotation;
        _controller.enabled = true;
        canMove = false;
        isPlaying = true;
        Chair = interactObject;
        interactObject.GetComponent<IInteractable>().Unhighlight();

        interactableLayers = LayerMask.GetMask("Card");

        playerState = PlayerState.Playing;
    }

    private void OnExitChair()
    {
        if (!isPlaying)
            return;

        _controller.enabled = false;
        transform.position = Chair.GetComponent<Chair>().GetExitPoint();
        _controller.enabled = true;

        Chair = null;

        canMove = true;
        isPlaying = false;

        interactableLayers = LayerMask.GetMask("Interactable");

        playerState = PlayerState.Walking;
    }

    private void OnPlayCards()
    {
        Chair.GetComponent<Chair>().PlayCards();
    }

    public void OnToggleCursor()
    {
        if (!cursorEnabled)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            cursorEnabled = true;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            cursorEnabled = false;
        }
    }

    public void RemoveInteractObject()
    {
        if (interactObject)
            interactObject.GetComponent<IInteractable>().Highlight();

        interactObject = null;
    }

    private void OnApplicationFocus(bool hasFocus) => SetCursorState(cursorLocked);
    private void SetCursorState(bool newState) => Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
}
