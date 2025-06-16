using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class CharacterMotor : NetworkBehaviour
{
    [Header("Character Identity")]
    [Tooltip("Imposta questo nell'inspector: CharacterA o CharacterB")]
    [SerializeField] private CharacterID characterId;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;


    [SerializeField][ReadOnly] private PlayerInputData currentInput;
    [SerializeField][ReadOnly] private GameMode gameMode;

    public override void OnNetworkSpawn()
    {
        // Esegui solo sul proprietario di questo oggetto
        if (!IsOwner) return;

        PlayerInput input = GetComponent<PlayerInput>();
        input.enabled = true;
        //input.actions = inputActions;

        // Determina quale personaggio controllare in base all'ID del client.
        // L'Host (ClientId 0) controlla A, il primo client che si unisce (ClientId 1) controlla B.
        // Questa logica può essere resa più complessa (es. scelta nel lobby).
        CharacterID controlledCharacter = (OwnerClientId == 0) ? CharacterID.CharacterA : CharacterID.CharacterB;

        // Registra questo giocatore online con l'InputManager
        InputManager.Instance.RegisterLocalPlayer((int)OwnerClientId, controlledCharacter, input);
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;

        InputManager.Instance.UnregisterPlayer((int)OwnerClientId);
    }

    private void Start()
    {
        gameMode = GameManager.Instance.gameMode;

        if (gameMode == GameMode.OnlineMultiplayer && !IsOwner)
        {
            return;
        }
    }

    void Update()
    {
        if (gameMode == GameMode.OnlineMultiplayer && !IsOwner)
        {
            return;
        }

        // Chiede costantemente all'InputManager i suoi input
        currentInput = InputManager.Instance.GetInputForCharacter(characterId, gameMode);

        // Applica gli input
        HandleMovement();
        HandleActions();
    }

    private void HandleMovement()
    {
        if (currentInput.Move != Vector2.zero)
        {
            Vector3 movement = new Vector3(currentInput.Move.x, 0, 0);
            transform.Translate(movement * moveSpeed * Time.deltaTime);
        }
    }

    private void HandleActions()
    {
        if (currentInput.JumpPressed)
        {
            Debug.Log($"{characterId} is Jumping!");
            // Aggiungi qui la tua logica di salto
        }
        if (currentInput.FirePressed)
        {
            Debug.Log($"{characterId} is Firing!");
            // Aggiungi qui la tua logica di sparo
        }
    }
}