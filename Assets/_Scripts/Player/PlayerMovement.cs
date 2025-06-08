using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputs))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Player Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField][ReadOnly] private float gravity = 9.81f;

    private CharacterController characterController;
    private PlayerInputs playerInputs;

    [Header("Debugging Variables")]
    [SerializeField][ReadOnly] private Vector3 moveDirection;
    [SerializeField][ReadOnly] private bool isJumpingRequest;


    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerInputs = GetComponent<PlayerInputs>();
    }

    private void Update()
    {
        float inputX = playerInputs.MovementInput.x;
        if (playerInputs.JumpInput && characterController.isGrounded)
        {
            isJumpingRequest = true;
        }

        MovePlayer(inputX);
    }

    private void MovePlayer(float horizontalInput)
    {
        // 1) Spostamento orizzontale
        Vector3 horizontalMove = new Vector3(horizontalInput * moveSpeed, 0f, 0f);

        // 2) Gestione salto / gravità
        if (characterController.isGrounded)
        {
            playerInputs.JumpInput = false;

            // Se ero in aria, azzero il velocity.y prima di restare fermo a terra
            if (moveDirection.y < 0f)
                moveDirection.y = -2f; // piccolo “appoggio” per far restare aderente

            if (isJumpingRequest)
            {
                // v = sqrt(2 * g * h)
                moveDirection.y = Mathf.Sqrt(2f * gravity * jumpHeight);
                isJumpingRequest = false;
            }
        }
        else
        {
            // applico gravità nel tempo
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // 3) Sommo vettori e muovo il CharacterController
        Vector3 finalMove = horizontalMove + Vector3.up * moveDirection.y;
        characterController.Move(finalMove * Time.deltaTime);
    }
}
