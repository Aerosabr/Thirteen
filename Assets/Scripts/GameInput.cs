using UnityEngine;
using System;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    private PlayerInput playerInput;

    public event EventHandler OnInteractAction;
    public event EventHandler OnExitChairAction;
    public event EventHandler OnPlayCardsAction;
    public event EventHandler OnAlternateInteractAction;

    private void Awake()
    {
        Instance = this;

        playerInput = new PlayerInput();
        playerInput.Player.Enable();
        playerInput.Player.Interact.performed += Interact_performed;
        playerInput.Player.ExitChair.performed += ExitChair_performed;
        playerInput.Player.PlayCards.performed += PlayCards_performed;
        playerInput.Player.AlternateInteract.performed += AlternateInteract_performed;
    }

    private void OnDestroy()
    {
        playerInput.Player.Interact.performed -= Interact_performed;

        playerInput.Dispose();
    }

    private void Interact_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnInteractAction?.Invoke(this, EventArgs.Empty);
    }

    private void ExitChair_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnExitChairAction?.Invoke(this, EventArgs.Empty);
    }

    private void PlayCards_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnPlayCardsAction?.Invoke(this, EventArgs.Empty);
    }

    private void AlternateInteract_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnAlternateInteractAction?.Invoke(this, EventArgs.Empty);
    }

    public Vector2 GetMovementVector()
    {
        Vector2 inputVector = playerInput.Player.Move.ReadValue<Vector2>();

        inputVector = inputVector.normalized;

        return inputVector;
    }

    public Vector2 GetLookVector()
    {
        Vector2 lookVector = playerInput.Player.Look.ReadValue<Vector2>();
        return lookVector;
    }
}
