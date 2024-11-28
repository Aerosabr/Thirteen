using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using Unity.Netcode;

[RequireComponent(typeof(CharacterController))]
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

    [SerializeField] private int interactableLayer;
    [SerializeField] private GameObject cinemachineCameraTarget;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private AudioListener audioListener;

    private float _cinemachineTargetPitch;
    private float _speed;
    private float _rotationVelocity;

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

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            audioListener.enabled = true;
            virtualCamera.Priority = 1;
        }
        else
        {
            virtualCamera.Priority = 0;
        }
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        Move();
        HighlightObject();
    }

    private void LateUpdate()
    {
        if (!IsOwner)
            return;

        if (canLook)
            CameraRotation();
    }

    #region Camera, Movement, Interaction
    private void CameraRotation()
    {
        float _threshold = 0.01f;
        lookInput = GameInput.Instance.GetLookVector();

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

        movementInput = GameInput.Instance.GetMovementVector();

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

        if (Physics.Raycast(ray, out hit, interactionDistance, 1 << interactableLayer))
        {
            if (!interactObject)
            {
                interactObject = hit.collider.gameObject;
                interactObject.GetComponent<IInteractable>().Highlight(gameObject);
            }
            else if (interactObject != hit.collider.gameObject)
            {
                interactObject.GetComponent<IInteractable>().Unhighlight();
                interactObject = hit.collider.gameObject;
                interactObject.GetComponent<IInteractable>().Highlight(gameObject);
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
    public void OnSprint(InputValue value) => isSprinting = value.isPressed;
    public void OnInteract(InputValue value)
    {
        if (interactObject && canInteract)
            interactObject.GetComponent<IInteractable>().InteractServerRpc(NetworkObject);
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

    [ServerRpc(RequireOwnership = false)]
    public override void SitOnChairServerRpc(NetworkObjectReference chairRef)
    {
        if (playerState == PlayerState.Sitting)
            return;

        chairRef.TryGet(out NetworkObject chairObj);
        Chair chair = chairObj.GetComponent<Chair>();

        Debug.Log(chair.name + " " + chair.GetSitPoint().transform.position);

        // Update position and rotation on the server
        _controller.enabled = false;
        transform.position = chair.GetSitPoint().transform.position;
        transform.rotation = chair.GetSitPoint().transform.rotation;
        _controller.enabled = true;

        this.chair = chair;
        canMove = false;

        playerVisual.LoadModel(ThirteenMultiplayer.Instance.GetPlayerDataIndexFromClientId(NetworkManager.Singleton.LocalClientId));

        interactableLayer = LayerMask.NameToLayer("Player" + playerID);

        ChangePlayerState(PlayerState.Sitting);

        // Call a ClientRpc to notify clients
        SitOnChairClientRpc(transform.position, transform.rotation);
    }

    [ClientRpc]
    private void SitOnChairClientRpc(Vector3 position, Quaternion rotation)
    {
        // Update the client's position and rotation
        _controller.enabled = false;
        transform.position = position;
        transform.rotation = rotation;
        _controller.enabled = true;
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

        interactableLayer = LayerMask.NameToLayer("Interactable");

        ChangePlayerState(PlayerState.Idle);
    }

    private void OnPlayCards()
    {
        if (playerState == PlayerState.Sitting && canPlay)
        {
            if (selectedCards.Count == 0)
            {
                if (Table.Instance.GetCurrentType() != CardType.LowestThree && Table.Instance.GetCurrentType() != CardType.Any)
                {
                    Table.Instance.SkipTurn();
                    canPlay = false;
                }
            }
            else if (Table.Instance.CheckIfCardsValid(selectedCards))
            {
                canPlay = false;
                canInteract = false;
                playerVisual.PlayAnimation("Throwing");
            }
        }

        if (StartNextGameUI.Instance.GetAwaitingReady())
            StartNextGameUI.Instance.ReadyUp(this);
            
    }

    public override void CardThrown()
    {
        Table.Instance.PlayCards(selectedCards);
        selectedCards.Clear();
        chair.CardsPlayed();

        canInteract = true;
    }

    public void OnEnableCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        cursorEnabled = true;
        canLook = false;
    }

    public void OnDisableCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        cursorEnabled = false;
        canLook = true;
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

    [ServerRpc(RequireOwnership = false)]
    public override void InitializePlayerServerRpc(int playerPos)
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        canLook = true;
        canInteract = true;

        var children = transform.GetComponentsInChildren<Transform>(includeInactive: true);
        foreach (var child in children)
            child.gameObject.layer = LayerMask.NameToLayer("Player" + playerPos + "Blank"); 

        int excludedLayers = 0;
        for (int i = 1; i <= 4; i++)
        {
            if (i == playerPos)
                excludedLayers |= 1 << LayerMask.NameToLayer("Player" + i + "Blank");
            else
                excludedLayers |= 1 << LayerMask.NameToLayer("Player" + i);
        }

        _mainCamera.GetComponent<Camera>().cullingMask = ~0 & ~excludedLayers;

        playerID = playerPos;
        Table.Instance.GetChair(playerPos).InteractServerRpc(NetworkObject);
        Table.Instance.OnPlayerTurn += Table_OnPlayerTurn;
    }

    private void Table_OnPlayerTurn(object sender, Table.OnPlayerTurnEventArgs e)
    {
        if (e.currentPlayer == playerID)
            canPlay = true;
    }
}
