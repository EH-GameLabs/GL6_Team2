using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Configuration")]
    [ReadOnly] public GameMode gameMode;
    [SerializeField] private GameObject playerControllerPrefab; // Prefab con solo PlayerInput

    [Header("Character Prefabs")]
    [SerializeField] private GameObject characterAPrefab;
    [SerializeField] private GameObject characterBPrefab;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetupGame(GameMode mode)
    {
        InputManager.Instance.SetGameMode(mode);

        switch (mode)
        {
            case GameMode.SinglePlayer:
                // Istanzia un solo controller di input e lo registra per il single player (ID 0)
                var playerInput = PlayerInput.Instantiate(playerControllerPrefab, controlScheme: "Keyboard&Mouse", pairWithDevice: Keyboard.current);
                InputManager.Instance.RegisterPlayer(0, CharacterID.CharacterA, playerInput); // Inizia controllando A
                GameStateManager.Instance.CurrentGameState = GameState.Playing;
                break;

            case GameMode.LocalMultiplayer:
                // Gestisco con la funzione: "SetupLocalMultiplayerGame"
                break;

            case GameMode.OnlineMultiplayer:

                AddNetworkComponents(characterAPrefab);
                AddNetworkComponents(characterBPrefab);

                // La logica di setup online è gestita da Netcode, Lobby, etc.
                // Il GameManager qui potrebbe solo settare la modalità.
                // Lo spawn e la registrazione verranno gestiti da un altro script (vedi sotto).
                break;
        }
    }

    private void AddNetworkComponents(GameObject tmp)
    {
        if (!tmp.GetComponent<NetworkObject>().IsSpawned)
        {
            tmp.GetComponent<NetworkObject>().Spawn();
        }

        NetworkTransform networkTransform = tmp.GetComponent<NetworkTransform>();

        networkTransform.SyncPositionZ = false;

        networkTransform.SyncScaleX = false;
        networkTransform.SyncScaleY = false;
        networkTransform.SyncScaleZ = false;

        //networkTransform.SyncRotAngleX = false;
        //networkTransform.SyncRotAngleY = false;
        //networkTransform.SyncRotAngleZ = false;

        //tmp.AddComponent<NetworkAnimator>();

        Debug.Log("Added Network components to: " + tmp.name);
    }

    public void SetupLocalMultiplayerGame(int pad1Index, int pad2Index)
    {
        InputManager.Instance.SetGameMode(GameMode.LocalMultiplayer);

        var pim = gameObject.GetComponent<PlayerInputManager>();

        // Crea PlayerInput per il primo giocatore
        PlayerInput player1Input = PlayerInput.Instantiate(playerControllerPrefab, controlScheme: "Gamepad", pairWithDevice: Gamepad.all[pad1Index]);
        InputManager.Instance.RegisterPlayer(0, CharacterID.CharacterA, player1Input);

        // Crea PlayerInput per il secondo giocatore
        PlayerInput player2Input = PlayerInput.Instantiate(playerControllerPrefab, controlScheme: "Gamepad", pairWithDevice: Gamepad.all[pad2Index]);
        InputManager.Instance.RegisterPlayer(1, CharacterID.CharacterB, player2Input);

        GameStateManager.Instance.CurrentGameState = GameState.Playing;
    }
}