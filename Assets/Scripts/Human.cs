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
    }

    #region Camera & Movement
    private float moveSpeed = 4.0f;
    private float sprintSpeed = 6.0f;
    private float cameraSensitivity = 1.5f;
    private float speedChangeRate = 10.0f;
    private float interactionDistance = 2.5f;

    [SerializeField] private LayerMask interactableLayers;
    [SerializeField] private GameObject cinemachineCameraTarget;

    private float _cinemachineTargetPitch;
    private float _speed;
    private float _rotationVelocity;

    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private CharacterController _controller;
    [SerializeField] private GameObject _mainCamera;

    private Vector2 movementInput;
    private Vector2 lookInput;
    private bool isSprinting;
    #endregion

    private bool canPlay;
    private bool canInteract;
    private bool canMove;
    private bool canLook;
    private bool cursorEnabled;
    private PlayerState playerState;

    [SerializeField] private GameObject interactObject;
    [SerializeField] private List<Card> selectedCards;

    private void Awake()
    {
        playerState = PlayerState.Idle;
    }

    private void Start()
    {

    }

    private void Update()
    {
        Move();
        HighlightObject();
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    #region Camera, Movement, Interaction
    private void CameraRotation()
    {
        if (!canLook)
            return;

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
        if (!canMove)
            return;

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
            ChangePlayerState(PlayerState.Walking);
            inputDirection = transform.right * movementInput.x + transform.forward * movementInput.y;
        }
        else
            ChangePlayerState(PlayerState.Idle);

        _controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + Vector3.zero * Time.deltaTime);
    }

    private void HighlightObject()
    {
        if (!canInteract)
            return;

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
    #endregion

    #region Player Inputs
    public void OnMove(InputValue value) => movementInput = value.Get<Vector2>();
    public void OnLook(InputValue value)
    {
        if (!cursorEnabled)
            lookInput = value.Get<Vector2>();
    }
    public void OnSprint(InputValue value) => isSprinting = value.isPressed;
    public void OnInteract(InputValue value)
    {
        if (interactObject && canInteract)
            interactObject.GetComponent<IInteractable>().Interact(gameObject);
    }
    #endregion

    #region Object Interaction
    public void SelectedCard(Card card)
    {
        if (selectedCards.Contains(card))
            selectedCards.Remove(card);
        else
            selectedCards.Add(card);
    }

    public override void SitOnChair(Chair chair)
    {
        if (playerState == PlayerState.Sitting)
            return;

        _controller.enabled = false;
        transform.position = chair.GetSitPoint().transform.position;
        transform.rotation = chair.GetSitPoint().transform.rotation;
        _controller.enabled = true;

        this.chair = chair;
        canMove = false;

        interactableLayers = LayerMask.GetMask("Card");

        ChangePlayerState(PlayerState.Sitting);
    }
    #endregion

    #region Other Inputs
    private void OnExitChair()
    {
        if (playerState != PlayerState.Sitting)
            return;

        _controller.enabled = false;
        transform.position = chair.GetExitPoint();
        _controller.enabled = true;

        chair = null;
        canMove = true;

        interactableLayers = LayerMask.GetMask("Interactable");

        ChangePlayerState(PlayerState.Idle);
    }

    private void OnPlayCards()
    {
        if (playerState == PlayerState.Sitting && canPlay)
        {
            if (selectedCards.Count == 0)
            {
                Table.Instance.SkipTurn();
                canPlay = false;
            }
            else if (Table.Instance.CheckIfCardsValid(selectedCards))
            {
                canPlay = false;
                canInteract = false;
                playerVisual.PlayAnimation("Throwing");
            }
        }
    }

    public override void CardThrown()
    {
        Table.Instance.PlayCards(selectedCards);
        selectedCards.Clear();
        chair.CardsPlayed();

        // no cards left

        canInteract = true;
    }

    public void OnToggleCursor()
    {
        if (!cursorEnabled)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            cursorEnabled = true;
            canLook = false;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            cursorEnabled = false;
            canLook = true;
        }
    }
    #endregion

    private void ChangePlayerState(PlayerState state)
    {
        if (playerState != state)
        {
            playerState = state;
            playerVisual.PlayAnimation(state.ToString());
        }
    }

    public override void InitializePlayer(int playerPos)
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        canLook = true;
        canInteract = true;

        GetComponent<CharacterController>().enabled = true;
        GetComponent<PlayerInput>().enabled = true;
        cinemachineCameraTarget.SetActive(true);

        playerID = playerPos;
        Table.Instance.GetChair(playerPos).Interact(gameObject);
        Table.Instance.OnPlayerTurn += Table_OnPlayerTurn;
    }

    private void Table_OnPlayerTurn(object sender, Table.OnPlayerTurnEventArgs e)
    {
        if (e.currentPlayer == playerID)
            canPlay = true;
    }
}
