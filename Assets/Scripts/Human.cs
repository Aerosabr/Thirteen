using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class Human : Player
{
    private enum PlayerState
    {
        Sitting,
        Walking,
        Idle,
        Throwing,
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

    private bool canLook = true;
    private PlayerState playerState;

    private float _cinemachineTargetPitch;
    private float _speed;
    private float _rotationVelocity;
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private CharacterController _controller;
    [SerializeField] private GameObject _mainCamera;

    [SerializeField] private GameObject interactObject;

    private void Awake()
    {
        playerState = PlayerState.Idle;
    }

    private void Start()
    {

    }

    private void Update()
    {
        if (playerState == PlayerState.Walking)
            Move();

        if (playerState != PlayerState.Idle)
            InteractWithObject();
    }

    private void LateUpdate()
    {
        if (playerState != PlayerState.Idle && !cursorEnabled)
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
        {
            playerVisual.PlayAnimation("Walking");
            inputDirection = transform.right * movementInput.x + transform.forward * movementInput.y;
        }
        else
            playerVisual.PlayAnimation("Idle");

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
            if (playerState == PlayerState.Sitting)
                interactObject.GetComponent<IInteractable>().Interact(chair.gameObject);
            else if (playerState == PlayerState.Walking)
                interactObject.GetComponent<IInteractable>().Interact(gameObject);
        }
    }

    public override void SitOnChair(Chair chair)
    {
        _controller.enabled = false;
        transform.position = chair.GetSitPoint().transform.position;
        transform.rotation = chair.GetSitPoint().transform.rotation;
        _controller.enabled = true;

        this.chair = chair;

        interactableLayers = LayerMask.GetMask("Card");

        playerState = PlayerState.Sitting;
        playerVisual.PlayAnimation("Sitting");
    }

    private void OnExitChair()
    {
        if (playerState != PlayerState.Sitting)
            return;

        _controller.enabled = false;
        transform.position = chair.GetExitPoint();
        _controller.enabled = true;

        chair = null;

        interactableLayers = LayerMask.GetMask("Interactable");

        playerState = PlayerState.Walking;
        playerVisual.PlayAnimation("Idle");
    }

    private void OnPlayCards()
    {
        if (playerState == PlayerState.Sitting)
        {
            if (chair.PlayCards())
                playerVisual.PlayAnimation("Throwing");
        }
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

    public override void InitializePlayer(int playerPos)
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        GetComponent<CharacterController>().enabled = true;
        GetComponent<PlayerInput>().enabled = true;
        cinemachineCameraTarget.SetActive(true);
        playerID = playerPos;
        Table.Instance.GetChair(playerPos).Interact(gameObject);
    }
}
