using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    // Variabile per lo switch in single-player
    private CharacterID activeSinglePlayerCharacter = CharacterID.CharacterA;

    // Dizionari per mappare i giocatori agli input e ai personaggi
    private Dictionary<int, PlayerInput> playerInputs = new Dictionary<int, PlayerInput>();
    private Dictionary<int, CharacterID> playerCharacterMap = new Dictionary<int, CharacterID>();
    private Dictionary<int, PlayerInputData> playerInputData = new Dictionary<int, PlayerInputData>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // --- METODI PUBBLICI DI CONFIGURAZIONE ---

    public void RegisterLocalPlayer(int playerId, CharacterID characterId, PlayerInput input)
    {
        playerInputs[playerId] = input;
        playerCharacterMap[playerId] = characterId;
        playerInputData[playerId] = new PlayerInputData(); // Inizializza i dati di input

        // Iscriviti agli eventi di input di questo specifico giocatore
        SubscribeToPlayerInputEvents(input, playerId);
        Debug.Log($"Player {playerId} registered to control {characterId}");
    }

    public void RegisterPlayer(int playerId, CharacterID characterId, PlayerInput input)
    {
        playerInputs[playerId] = input;
        playerCharacterMap[playerId] = characterId;
        playerInputData[playerId] = new PlayerInputData();

        // Configura auto-switch
        input.neverAutoSwitchControlSchemes = false;

        // Configura dispositivi
        ConfigureAllDevices(input);

        // Ascolta i cambi di schema
        input.onControlsChanged += (pi) => OnControlSchemeChanged(pi, playerId);

        SubscribeToPlayerInputEvents(input, playerId);
        Debug.Log($"Player {playerId} registered with auto control scheme switching");
    }

    private void OnControlSchemeChanged(PlayerInput playerInput, int playerId)
    {
        string currentScheme = playerInput.currentControlScheme;
        Debug.Log($"Player {playerId} switched to control scheme: {currentScheme}");

        //// Aggiorna UI o altri sistemi basati sul nuovo schema
        //UpdateUIForControlScheme(playerId, currentScheme);
        //UpdateInputPrompts(playerId, currentScheme);
    }

    private void ConfigureAllDevices(PlayerInput playerInput)
    {
        // Pair con tutti i dispositivi disponibili
        var user = playerInput.user;
        user.UnpairDevices();

        foreach (var device in InputSystem.devices)
        {
            if (device is Keyboard || device is Mouse || device is Gamepad)
            {
                InputUser.PerformPairingWithDevice(device, user);
            }
        }
    }

    public void UnregisterPlayer(int playerId)
    {
        if (playerInputs.TryGetValue(playerId, out PlayerInput input))
        {
            UnsubscribeFromPlayerInputEvents(input, playerId);
            playerInputs.Remove(playerId);
            playerCharacterMap.Remove(playerId);
            playerInputData.Remove(playerId);
            Debug.Log($"Player {playerId} unregistered.");
        }
    }

    // --- IL METODO CHIAVE PER I PERSONAGGI ---

    /// <summary>
    /// Qualsiasi script di un personaggio chiama questo metodo per ottenere i suoi input.
    /// </summary>
    public PlayerInputData GetInputForCharacter(CharacterID characterId, GameMode currentGameMode)
    {
        switch (currentGameMode)
        {
            case GameMode.SinglePlayer:
                // Se il personaggio che chiede input è quello attivo, ritorna l'input del giocatore 0.
                if (characterId == activeSinglePlayerCharacter)
                {
                    return playerInputData.TryGetValue(0, out var data) ? data : new PlayerInputData();
                }
                break;

            case GameMode.LocalMultiplayer:
            case GameMode.OnlineMultiplayer:
                // Trova il giocatore che è mappato a questo personaggio
                foreach (var entry in playerCharacterMap)
                {
                    if (entry.Value == characterId)
                    {
                        // Ritorna i dati di input di quel giocatore
                        return playerInputData.TryGetValue(entry.Key, out var data) ? data : new PlayerInputData();
                    }
                }
                break;
        }

        // Se nessuna condizione è soddisfatta, ritorna un input "vuoto" (nessuna azione)
        return new PlayerInputData();
    }

    // --- GESTIONE DEGLI EVENTI DI INPUT ---

    private void SubscribeToPlayerInputEvents(PlayerInput input, int playerId)
    {
        var actions = input.actions;
        actions["Move"].performed += ctx => HandleMove(ctx, playerId);
        actions["Move"].canceled += ctx => HandleMove(ctx, playerId);
        actions["Jump"].performed += ctx => HandleJump(ctx, playerId);
        actions["Jump"].canceled += ctx => HandleJump(ctx, playerId);
        actions["Interact"].performed += ctx => HandleInteract(ctx, playerId);
        actions["Interact"].canceled += ctx => HandleInteract(ctx, playerId);
        actions["SwitchCharacter"].performed += ctx => HandleSwitchCharacter(playerId);
    }

    // Esempio per Unsubscribe (meno critico se gli oggetti vengono distrutti correttamente)
    private void UnsubscribeFromPlayerInputEvents(PlayerInput input, int playerId) { /* ... logica di unsubscribe ... */ }

    // --- HANDLERS PRIVATI ---

    private void HandleMove(InputAction.CallbackContext context, int playerId)
    {
        var data = playerInputData[playerId];
        data.Move = context.ReadValue<Vector2>();
        playerInputData[playerId] = data;
    }

    private void HandleJump(InputAction.CallbackContext context, int playerId)
    {
        var data = playerInputData[playerId];
        data.JumpPressed = context.ReadValueAsButton();
        playerInputData[playerId] = data;
    }

    private void HandleInteract(InputAction.CallbackContext context, int playerId)
    {
        var data = playerInputData[playerId];
        data.FirePressed = context.ReadValueAsButton();
        playerInputData[playerId] = data;
    }

    private void HandleSwitchCharacter(int playerId)
    {
        // Lo switch ha senso solo in single player e per il giocatore 0
        if (GameManager.Instance.gameMode == GameMode.SinglePlayer && playerId == 0)
        {
            activeSinglePlayerCharacter = (activeSinglePlayerCharacter == CharacterID.CharacterA)
                ? CharacterID.CharacterB
                : CharacterID.CharacterA;
            Debug.Log($"Active character switched to: {activeSinglePlayerCharacter}");
        }
    }
}