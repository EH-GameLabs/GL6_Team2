#region OLD
//using System;
//using Unity.Netcode;
//using UnityEngine;
//using UnityEngine.InputSystem;
//using UnityEngine.Windows;

//public class CharacterMotor : NetworkBehaviour
//{
//    [Header("Character Identity")]
//    [Tooltip("Imposta questo nell'inspector: CharacterA o CharacterB")]
//    [SerializeField] private CharacterID characterId;

//    [Header("Movement Settings")]
//    [SerializeField] private float moveSpeed = 5f;

//    [SerializeField][ReadOnly] private PlayerInputData currentInput;
//    [SerializeField][ReadOnly] private GameMode gameMode;

//    public override void OnNetworkSpawn()
//    {
//        // Esegui solo sul proprietario di questo oggetto
//        if (!IsOwner) return;

//        PlayerInput input = GetComponent<PlayerInput>();
//        input.enabled = true;
//        //input.actions = inputActions;

//        // Determina quale personaggio controllare in base all'ID del client.
//        // L'Host (ClientId 0) controlla A, il primo client che si unisce (ClientId 1) controlla B.
//        // Questa logica pu� essere resa pi� complessa (es. scelta nel lobby).
//        CharacterID controlledCharacter = (OwnerClientId == 0) ? CharacterID.CharacterA : CharacterID.CharacterB;

//        // Registra questo giocatore online con l'InputManager
//        InputManager.Instance.RegisterLocalPlayer((int)OwnerClientId, controlledCharacter, input);
//    }

//    public override void OnNetworkDespawn()
//    {
//        if (!IsOwner) return;

//        InputManager.Instance.UnregisterPlayer((int)OwnerClientId);
//    }

//    private void Start()
//    {
//        gameMode = GameManager.Instance.gameMode;

//        if (gameMode == GameMode.OnlineMultiplayer && !IsOwner)
//        {
//            return;
//        }
//    }

//    void Update()
//    {
//        if (gameMode == GameMode.OnlineMultiplayer && !IsOwner)
//        {
//            return;
//        }

//        // Chiede costantemente all'InputManager i suoi input
//        currentInput = InputManager.Instance.GetInputForCharacter(characterId, gameMode);

//        // Applica gli input
//        switch (characterId)
//        {
//            case CharacterID.CharacterA:
//                HandleMovementSpidy();
//                break;
//            case CharacterID.CharacterB:
//                HandleMovementCandly();
//                break;
//        }

//        HandleActions();
//    }

//    private void HandleMovementSpidy()
//    {
//        if (currentInput.Move != Vector2.zero)
//        {
//            Vector3 movement = new Vector3(currentInput.Move.x, 0, 0);
//            transform.Translate(movement * moveSpeed * Time.deltaTime);
//        }
//    }

//    private void HandleMovementCandly()
//    {
//        throw new NotImplementedException();
//    }

//    private void HandleActions()
//    {
//        if (currentInput.JumpPressed)
//        {
//            Debug.Log($"{characterId} is Jumping!");
//            // Aggiungi qui la tua logica di salto
//        }
//        if (currentInput.FirePressed)
//        {
//            Debug.Log($"{characterId} is Firing!");
//            // Aggiungi qui la tua logica di sparo
//        }
//    }
//}
#endregion

// NEW
using System;
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
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float gravityScale = 2f;

    [Header("Ground Detection (Solo per CharacterA)")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayerMask = 1;

    [Header("Debug Info")]
    [SerializeField][ReadOnly] private PlayerInputData currentInput;
    [SerializeField][ReadOnly] private GameMode gameMode;
    [SerializeField][ReadOnly] private bool isGrounded;

    private Rigidbody rb;
    private SpriteRenderer spriteRenderer;

    public override void OnNetworkSpawn()
    {
        // Esegui solo sul proprietario di questo oggetto
        if (!IsOwner) return;

        PlayerInput input = GetComponent<PlayerInput>();
        input.enabled = true;

        // Determina quale personaggio controllare in base all'ID del client.
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

        // Ottieni il Rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError($"Rigidbody mancante su {gameObject.name}! Aggiungilo per il movimento fisico.");
            return;
        }

        // Configura il Rigidbody per platform 2D
        SetupRigidbody();

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (gameMode == GameMode.OnlineMultiplayer && !IsOwner)
        {
            return;
        }
    }

    private void SetupRigidbody()
    {
        // Blocca rotazione indesiderata per un platform 2D
        rb.freezeRotation = true;

        // Configura drag
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;

        // Configura in base al personaggio
        switch (characterId)
        {
            case CharacterID.CharacterA:
                // CharacterA � soggetto a gravit�
                rb.useGravity = true;
                break;

            case CharacterID.CharacterB:
                // CharacterB non � soggetto a gravit� (pu� volare)
                rb.useGravity = false;
                break;
        }
    }

    void FixedUpdate()
    {
        if (gameMode == GameMode.OnlineMultiplayer && !IsOwner)
        {
            return;
        }

        if (rb == null) return;

        // Controlla se � a terra (solo per CharacterA)
        if (characterId == CharacterID.CharacterA)
        {
            CheckGrounded();
            ApplyCustomGravity();
        }

        // Ottieni gli input
        currentInput = InputManager.Instance.GetInputForCharacter(characterId, gameMode);

        // Applica il movimento
        switch (characterId)
        {
            case CharacterID.CharacterA:
                HandleMovementSpidy();
                break;
            case CharacterID.CharacterB:
                HandleMovementCandly();
                break;
        }
    }

    void Update()
    {
        if (gameMode == GameMode.OnlineMultiplayer && !IsOwner)
        {
            return;
        }

        // Gestisci le azioni (non fisiche) in Update
        HandleActions();
    }

    private void HandleMovementSpidy()
    {
        // Movimento orizzontale
        if (Mathf.Abs(currentInput.Move.x) > 0.1f)
        {
            float targetVelocityX = currentInput.Move.x * moveSpeed;

            // Mantieni la velocita Y attuale, cambia solo X
            rb.linearVelocity = new Vector3(targetVelocityX, rb.linearVelocity.y, 0);

            // Flip del personaggio (opzionale)
            FlipCharacter(currentInput.Move.x);
        }
        else
        {
            // Ferma il movimento orizzontale mantenendo la velocita verticale
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    private void HandleMovementCandly()
    {
        // CharacterB si muove liberamente in 2D (senza gravita)
        Vector3 targetVelocity = Vector3.zero;

        // Movimento orizzontale
        if (Mathf.Abs(currentInput.Move.x) > 0.1f)
        {
            targetVelocity.x = currentInput.Move.x * moveSpeed;
            FlipCharacter(currentInput.Move.x);
        }

        // Movimento verticale
        if (Mathf.Abs(currentInput.Move.y) > 0.1f)
        {
            targetVelocity.y = currentInput.Move.y * moveSpeed;
        }

        // Applica la velocità target
        if (targetVelocity != Vector3.zero)
        {
            rb.linearVelocity = targetVelocity;
        }
        else
        {
            // Ferma gradualmente il movimento quando non ci sono input
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, Time.fixedDeltaTime * 8f);
        }
    }

    private void FlipCharacter(float horizontalInput)
    {
        if (horizontalInput > 0)
            spriteRenderer.flipX = false;
        else if (horizontalInput < 0)
            spriteRenderer.flipX = true;
    }

    private void CheckGrounded()
    {
        if (groundCheck == null)
        {
            // Fallback: usa la posizione del personaggio
            Vector3 checkPosition = transform.position - Vector3.up * 0.5f;
            isGrounded = Physics.CheckSphere(checkPosition, groundCheckRadius, groundLayerMask);
        }
        else
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayerMask);
        }
    }

    private void ApplyCustomGravity()
    {
        // Applica gravità extra per un salto più responsivo (solo per CharacterA)
        if (rb.linearVelocity.y < 0)
        {
            // Caduta più veloce
            rb.linearVelocity += (gravityScale - 1) * Physics.gravity.y * Time.fixedDeltaTime * Vector3.up;
        }
        else if (rb.linearVelocity.y > 0 && !currentInput.JumpPressed)
        {
            // Salto più corto se il pulsante non è tenuto premuto
            rb.linearVelocity += (gravityScale - 1) * Physics.gravity.y * Time.fixedDeltaTime * Vector3.up;
        }
    }

    private void HandleActions()
    {
        // Salto (solo per CharacterA quando � a terra)
        if (currentInput.JumpPressed)
        {
            currentInput.JumpPressed = false;
            if (characterId == CharacterID.CharacterA && isGrounded)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, 0);
                Debug.Log($"{characterId} is Jumping!");
            }
            else if (characterId == CharacterID.CharacterB)
            {
                // CharacterB potrebbe avere un'abilit� speciale invece del salto
                Debug.Log($"{characterId} special ability activated!");
            }
        }

        if (currentInput.FirePressed)
        {
            Debug.Log($"{characterId} is Firing!");
            // Aggiungi qui la tua logica di sparo
        }
    }

    // Metodo per creare automaticamente il ground check se mancante
    private void CreateGroundCheckIfMissing()
    {
        if (characterId == CharacterID.CharacterA && groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = Vector3.down * 0.5f;
            groundCheck = groundCheckObj.transform;
        }
    }

    // Debug Gizmos
    private void OnDrawGizmosSelected()
    {
        if (characterId == CharacterID.CharacterA)
        {
            Vector3 checkPosition = groundCheck != null ? groundCheck.position : transform.position - Vector3.up * 0.5f;
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(checkPosition, groundCheckRadius);
        }
    }

    // Metodo di utilit� per debugging
    [ContextMenu("Setup Ground Check")]
    private void SetupGroundCheck()
    {
        CreateGroundCheckIfMissing();
    }
}