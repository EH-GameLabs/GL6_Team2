using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

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
        playerInputData[playerId] = new PlayerInputData();

        // AGGIUNGI QUESTA LINEA - Abilita gli actions
        input.actions.Enable();

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
                return playerInputData.TryGetValue(0, out var data) ? data : new PlayerInputData();

            case GameMode.LocalMultiplayer:
            case GameMode.OnlineMultiplayer:
                // Trova il giocatore che � mappato a questo personaggio
                foreach (var entry in playerCharacterMap)
                {
                    if (entry.Value == characterId)
                    {
                        // Ritorna i dati di input di quel giocatore
                        return playerInputData.TryGetValue(entry.Key, out var dataa) ? dataa : new PlayerInputData();
                    }
                }
                break;
        }

        // Se nessuna condizione � soddisfatta, ritorna un input "vuoto" (nessuna azione)
        return new PlayerInputData();
    }

    // --- GESTIONE DEGLI EVENTI DI INPUT ---

    private void SubscribeToPlayerInputEvents(PlayerInput input, int playerId)
    {
        var actions = input.actions;

        // Debug per verificare che gli actions esistano
        //Debug.Log($"Available actions for player {playerId}: {string.Join(", ", actions.actionMaps.SelectMany(am => am.actions).Select(a => a.name))}");

        // Verifica che gli actions siano abilitati
        //Debug.Log($"Actions enabled for player {playerId}: {actions.enabled}");

        actions["Move"].performed += ctx =>
        {
            //Debug.Log($"Player {playerId} Move performed: {ctx.ReadValue<Vector2>()}");
            HandleMove(ctx, playerId);
        };

        actions["Move"].canceled += ctx => HandleMove(ctx, playerId);
        actions["Jump"].performed += ctx =>
        {
            //Debug.Log($"Player {playerId} Jump performed");
            HandleJump(ctx, playerId);
        };
        actions["Jump"].canceled += ctx => HandleJump(ctx, playerId);

        // ... altri actions
        actions["Interact"].performed += ctx => HandleInteract(ctx, playerId);
        actions["Interact"].canceled += ctx => HandleInteract(ctx, playerId);

        if (GameManager.Instance.gameMode != GameMode.SinglePlayer) return;
        actions["Look"].performed += ctx =>
        {
            //Debug.Log($"Player {playerId} Look performed: {ctx.ReadValue<Vector2>()}");
            var data = playerInputData[playerId];
            data.Look = ctx.ReadValue<Vector2>();
            playerInputData[playerId] = data;
        };
        actions["Look"].canceled += ctx =>
        {
            //Debug.Log($"Player {playerId} Look canceled");
            var data = playerInputData[playerId];
            data.Look = Vector2.zero; // Reset look when canceled
            playerInputData[playerId] = data;
        };
        //actions["SwitchCharacter"].performed += ctx => HandleSwitchCharacter(playerId);
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

        // Rileva solo quando il pulsante viene premuto (non tenuto premuto)
        if (context.performed) // Questo si attiva solo una volta quando premi
        {
            data.FirePressed = !data.FirePressed;
            Debug.Log($"Interact pressed: {data.FirePressed}");
        }

        playerInputData[playerId] = data;
    }

    //private void HandleSwitchCharacter(int playerId)
    //{
    //    // Lo switch ha senso solo in single player e per il giocatore 0
    //    if (GameManager.Instance.gameMode == GameMode.SinglePlayer && playerId == 0)
    //    {
    //        activeSinglePlayerCharacter = (activeSinglePlayerCharacter == CharacterID.CharacterA)
    //            ? CharacterID.CharacterB
    //            : CharacterID.CharacterA;
    //        Debug.Log($"Active character switched to: {activeSinglePlayerCharacter}");
    //    }
    //}
}