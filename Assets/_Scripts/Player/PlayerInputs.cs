using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputs : MonoBehaviour
{
    #region MOVEMENT
    [SerializeField][ReadOnly] private Vector2 movementInput;
    public Vector2 MovementInput
    {
        get => movementInput;
        set => movementInput = value;
    }

    private void OnMove(InputValue value)
    {
        MovementInput = value.Get<Vector2>();
    }
    #endregion

    #region JUMP
    [SerializeField][ReadOnly] private bool jumpInput;
    public bool JumpInput
    {
        get => jumpInput;
        set => jumpInput = value;
    }

    private void OnJump(InputValue value)
    {
        JumpInput = value.isPressed;
    }
    #endregion

    #region INTERACT
    [SerializeField][ReadOnly] private bool interactInput;
    public bool InteractInput
    {
        get => interactInput;
        set => interactInput = value;
    }

    private void OnInteract(InputValue value)
    {
        InteractInput = value.isPressed;
    }
    #endregion
}
