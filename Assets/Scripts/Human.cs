using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using Unity.Netcode;
using TMPro;

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
    public bool canInteract;
    public bool canMove;
    private bool canLook;
    private bool cursorEnabled;
    private PlayerState playerState;
    [SerializeField] private PlayerInfo playerInfo;

    [SerializeField] private TextMeshProUGUI nametag;
    [SerializeField] private GameObject interactObject;
    [SerializeField] private List<Card> selectedCards;

    private void Awake()
    {
        playerState = PlayerState.Idle;
    }

    private void Start()
    {
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
        GameInput.Instance.OnExitChairAction += GameInput_OnExitChairAction;
        GameInput.Instance.OnPlayCardsAction += GameInput_OnPlayCardsAction;
        if (IsServer)
            GameInput.Instance.OnAlternateInteractAction += GameInput_OnAlternateInteractAction;
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("On network spawn: " + OwnerClientId);
        playerInfo = PlayerManager.Instance.GetPlayerInfoFromID(OwnerClientId);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        canMove = true;
        canLook = true;
        canInteract = true;
        interactObject = null;
        playerVisual.LoadModel(playerInfo.modelNum);
        nametag.text = playerInfo.playerName.ToString();
        /*
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
        */

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
            Look();
    }

    #region Camera, Movement, Interaction
    private void Look()
    {
        float _threshold = 0.01f;
        lookInput = GameInput.Instance.GetLookVector();

        if (lookInput.sqrMagnitude >= _threshold)
        {
            if (canMove)
            {
                float TopClamp = 90.0f;
                float BottomClamp = -90.0f;

                _cinemachineTargetPitch += lookInput.y * cameraSensitivity;
                _rotationVelocity = lookInput.x * cameraSensitivity;

                _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

                cinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

                transform.Rotate(Vector3.up * _rotationVelocity);
            }
            else
            {
                // Default clamp values for vertical (pitch) and horizontal (yaw)
                float TopClamp = 60.0f;
                float BottomClamp = -45.0f;
                float leftClamp = -60.0f;
                float rightClamp = 60.0f;

                // Update vertical look (pitch)
                _cinemachineTargetPitch += lookInput.y * cameraSensitivity;
                _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

                // Update horizontal look (yaw)
                float currentYaw = cinemachineCameraTarget.transform.localRotation.eulerAngles.y;
                float newYaw = currentYaw + (lookInput.x * cameraSensitivity);
                newYaw = ClampAngle(newYaw, leftClamp, rightClamp);

                // Apply pitch and yaw to cinemachineCameraTarget only
                cinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, newYaw, 0.0f);
            }

        }
    }

    private float ClampAngle(float angle, float min, float max)
    {
        angle = (angle + 360.0f) % 360.0f; // Normalize angle to 0-360
        if (angle > 180.0f) angle -= 360.0f; // Convert to -180 to 180
        return Mathf.Clamp(angle, min, max);
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

        _controller.Move(inputDirection.normalized * (_speed * Time.deltaTime));
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
            if (interactObject != null && interactObject != hit.collider.gameObject)
            {
                interactObject.GetComponent<InteractableObject>().Unhighlight();
                interactObject = null;
                InteractionUI.Instance.Hide();
            }

            if (!interactObject && hit.collider.gameObject.GetComponent<InteractableObject>().Highlight(gameObject))
            {
                interactObject = hit.collider.gameObject;
                InteractionUI.Instance.Show(interactObject.GetComponent<InteractableObject>().GetInteractType(), IsServer);
            }
        }
        else if (interactObject)
        {
            interactObject.GetComponent<InteractableObject>().Unhighlight();
            interactObject = null;
            InteractionUI.Instance.Hide();
        }
    }
    #endregion

    #region Player Inputs
    private void GameInput_OnInteractAction(object sender, System.EventArgs e)
    {
        if (!IsOwner)
            return;

        if (interactObject && canInteract)
            interactObject.GetComponent<InteractableObject>().Interact(NetworkObject);
    }

    private void GameInput_OnExitChairAction(object sender, System.EventArgs e)
    {
        if (playerState != PlayerState.Sitting || !IsOwner)
            return;

        chair.PlayerExitServerRpc(NetworkObject);
    }

    private void GameInput_OnPlayCardsAction(object sender, System.EventArgs e)
    {
        if (playerState == PlayerState.Sitting && canPlay && IsOwner)
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

    private void GameInput_OnAlternateInteractAction(object sender, System.EventArgs e)
    {
        if (!IsOwner)
            return;

        if (interactObject != null && 
            interactObject.GetComponent<InteractableObject>().GetInteractType() == InteractableObject.InteractType.Chair)
        {
            if (interactObject.GetComponent<Chair>().GetPlayerType() != PlayerType.Player)
                interactObject.GetComponent<Chair>().AlternateInteractServerRpc();
        }
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

    public override void SitOnChair(NetworkObjectReference chairRef)
    {
        if (playerState == PlayerState.Sitting)
            return;

        chairRef.TryGet(out NetworkObject chairObj);
        Chair chair = chairObj.GetComponent<Chair>();

        _controller.enabled = false;
        transform.position = chair.GetSitPoint().transform.position;
        transform.rotation = chair.GetSitPoint().transform.rotation;
        _controller.enabled = true;

        this.chair = chair;
        canMove = false;

        interactableLayer = LayerMask.NameToLayer("Player" + playerID);

        ToggleGameUI(true);

        ChangePlayerState(PlayerState.Sitting);
    }

    public override void ExitChair()
    {
        _controller.enabled = false;
        transform.position = chair.GetExitPoint();
        _controller.enabled = true;

        chair = null;
        canMove = true;

        interactableLayer = LayerMask.NameToLayer("Interactable");

        ToggleGameUI(false);

        ChangePlayerState(PlayerState.Idle);
    }
    #endregion

    #region Other Inputs
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

    private void ToggleGameUI(bool toggle)
    {
        if (!IsOwner)
            return;

        if (toggle)
        {
            GameStateUI.Instance.Show();
            PlayerOrderUI.Instance.Show();
            StartNextGameUI.Instance.Show();
        }
        else
        {
            GameStateUI.Instance.Hide();
            PlayerOrderUI.Instance.Hide();
            StartNextGameUI.Instance.Hide();
        }
    }

    private void ChangePlayerState(PlayerState state)
    {
        if (playerState != state)
        {
            playerState = state;
            playerVisual.PlayAnimation(state.ToString());
        }
    }

    private void Table_OnPlayerTurn(object sender, Table.OnPlayerTurnEventArgs e)
    {
        if (e.currentPlayer == playerID)
            canPlay = true;
    }
}
