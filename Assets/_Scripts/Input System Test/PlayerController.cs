using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class PlayerController : NetworkBehaviour
{
    [Header("Character Identity")]
    [Tooltip("Imposta questo nell'inspector: CharacterA o CharacterB")]
    public CharacterID characterId;

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
    private Grapple3D grapple3D;

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

        grapple3D = GetComponent<Grapple3D>();

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

        // Controlla se è a terra (solo per CharacterA)
        if (characterId == CharacterID.CharacterA)
        {
            if (grapple3D.isGrappling) return;
            CheckGrounded();
            ApplyCustomGravity();
            CheckOutOfTheMap();
            CheckPassablePlatform();
        }

        // Ottieni gli input
        currentInput = InputManager.Instance.GetInputForCharacter(characterId, gameMode);

        // Applica il movimento
        switch (characterId)
        {
            case CharacterID.CharacterA:
                if (grapple3D.isGrappling) return;
                HandleMovementSpidy();
                break;

            case CharacterID.CharacterB:
                if (gameMode == GameMode.SinglePlayer) return;
                HandleMovementCandly();
                break;
        }
    }

    private void LateUpdate()
    {
        if (characterId == CharacterID.CharacterA) return;

        if (gameMode == GameMode.OnlineMultiplayer && !IsOwner)
        {
            return;
        }

        if (gameMode == GameMode.SinglePlayer)
        {
            HandleMovementCandly();
        }

        StayInsideTheCamera();
    }

    private void StayInsideTheCamera()
    {
        // il personaggio deve rimanere all'interno della visuale della telecamera
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        Vector3 cameraPosition = mainCamera.transform.position;
        Vector3 cameraSize = mainCamera.orthographicSize * new Vector3(mainCamera.aspect, 1, 0);
        Vector3 clampedPosition = transform.position;

        clampedPosition.x = Mathf.Clamp(clampedPosition.x, cameraPosition.x - cameraSize.x, cameraPosition.x + cameraSize.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, cameraPosition.y - cameraSize.y, cameraPosition.y + cameraSize.y);

        transform.position = clampedPosition;
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
            if (isGrounded) rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    private void HandleMovementCandly()
    {
        if (GameManager.Instance.gameMode == GameMode.SinglePlayer)
        {
            CandlyMovement(currentInput.Look);
            return;
        }

        CandlyMovement(currentInput.Move);
    }

    private void CandlyMovement(Vector2 input)
    {
        if (GameManager.Instance.gameMode == GameMode.SinglePlayer)
        {
            Vector3 screenPoint = new Vector3(input.x, input.y, Camera.main.nearClipPlane);
            Vector3 world3d = Camera.main.ScreenToWorldPoint(screenPoint);
            Vector2 target = world3d;

            Vector2 current = rb.position;
            Vector2 newPos = Vector2.MoveTowards(current, target, moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);
            FlipCharacter(newPos.x - current.x);
            return;
        }


        // Modalità multiplayer (il tuo codice preesistente)
        Vector3 targetVelocity = Vector3.zero;
        if (Mathf.Abs(input.x) > 0.1f) { targetVelocity.x = input.x * moveSpeed; FlipCharacter(input.x); }
        if (Mathf.Abs(input.y) > 0.1f) { targetVelocity.y = input.y * moveSpeed; }
        if (targetVelocity != Vector3.zero) rb.linearVelocity = targetVelocity;
        else rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, Time.fixedDeltaTime * 8f);
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

    private void CheckPassablePlatform()
    {
        // Controlla se il personaggio sta attraversando una piattaforma passabile
        if (rb.linearVelocity.y > 0)
        {
            //Debug.Log($"Character {characterId} is moving up, checking for passable platforms.");

            // Versione migliorata con distanza limitata e controllo distanza minima
            float rayDistance = 2f; // Limita la distanza del raycast
            if (Physics.Raycast(transform.position, Vector3.up, out RaycastHit hit, rayDistance))
            {
                // Verifica che non sia troppo vicino (evita falsi positivi)
                if (hit.distance > 0.1f)
                {
                    // CORREZIONE: usa hit.collider invece di transform
                    if (hit.collider.TryGetComponent<PassablePlatform>(out PassablePlatform passablePlatform))
                    {
                        //Debug.Log($"Character {characterId} is passing through a passable platform: {hit.collider.name}");
                        passablePlatform.SetTrigger(true);
                    }
                }
            }
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitDown, rayDistance))
            {
                // Verifica che non sia troppo vicino (evita falsi positivi)
                if (hitDown.distance > 0.2f)
                {
                    // CORREZIONE: usa hitDown.collider invece di transform
                    if (hitDown.collider.TryGetComponent<PassablePlatform>(out PassablePlatform passablePlatformDown))
                    {
                        //Debug.Log($"Character {characterId} is passing through a passable platform: {hitDown.collider.name}");
                        passablePlatformDown.SetTrigger(false);
                    }
                }
            }
        }
    }

    private void CheckOutOfTheMap()
    {
        if (transform.position.y < -10f)
        {
            Debug.LogWarning($"Character {characterId} is out of the map! Resetting position.");
            transform.position = GameManager.Instance.player1SpawnPoint.position;

            GameManager.Instance.LoseLife(characterId);
        }
    }

    private void HandleActions()
    {
        // Salto (solo per CharacterA quando è a terra)
        if (currentInput.JumpPressed)
        {
            currentInput.JumpPressed = false;
            if (characterId == CharacterID.CharacterA && isGrounded)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, 0);
            }
            else if (characterId == CharacterID.CharacterB)
            {

            }
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