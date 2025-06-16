using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("GameObjects Prefabs")]
    public GameObject player1Prefab;
    public GameObject player2Prefab;
    public Transform player1SpawnPoint;
    public Transform player2SpawnPoint;

    [Header("Configuration")]
    [ReadOnly] public GameMode gameMode;
    //[SerializeField] private GameObject playerControllerPrefab; // Prefab con solo PlayerInput

    //[Header("Character Prefabs")]
    //[SerializeField] private GameObject characterAPrefab;
    //[SerializeField] private GameObject characterBPrefab;

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
        switch (mode)
        {
            case GameMode.SinglePlayer:
                GameObject g1 = Instantiate(player1Prefab, player1SpawnPoint.position, Quaternion.identity);
                g1.name = "Player1";
                GameObject g2 = Instantiate(player2Prefab, player2SpawnPoint.position, Quaternion.identity);
                g2.name = "Player2";

                PlayerInput playerInput = g1.GetComponent<PlayerInput>();
                playerInput.enabled = true;

                InputManager.Instance.RegisterPlayer(0, CharacterID.CharacterA, playerInput);

                GameStateManager.Instance.CurrentGameState = GameState.Playing;
                break;

            case GameMode.LocalMultiplayer:
                // Gestisco con la funzione: "SetupLocalMultiplayerGame"
                break;

            case GameMode.OnlineMultiplayer:

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
        // Verifica che i gamepad esistano
        if (pad1Index >= Gamepad.all.Count || pad2Index >= Gamepad.all.Count)
        {
            Debug.LogError("Not enough gamepads connected!");
            return;
        }

        GameObject p1 = Instantiate(player1Prefab, player1SpawnPoint.position, Quaternion.identity);
        GameObject p2 = Instantiate(player2Prefab, player2SpawnPoint.position, Quaternion.identity);

        if (!p1.TryGetComponent<PlayerInput>(out var player1Input))
        {
            player1Input = p1.AddComponent<PlayerInput>();
        }
        else
        {
            player1Input.enabled = true;
        }

        if (!p2.TryGetComponent<PlayerInput>(out var player2Input))
        {
            player2Input = p2.AddComponent<PlayerInput>();
        }
        else
        {
            player2Input.enabled = true;
        }


        ConfigurePlayerInput(player1Input, Gamepad.all[pad1Index], "Gamepad");
        ConfigurePlayerInput(player2Input, Gamepad.all[pad2Index], "Gamepad");

        // Registra i player
        InputManager.Instance.RegisterLocalPlayer(0, CharacterID.CharacterA, player1Input);
        InputManager.Instance.RegisterLocalPlayer(1, CharacterID.CharacterB, player2Input);

        GameStateManager.Instance.CurrentGameState = GameState.Playing;
    }

    private void ConfigurePlayerInput(PlayerInput playerInput, InputDevice device, string controlScheme)
    {
        // Rimuovi tutti i dispositivi esistenti
        var user = playerInput.user;
        user.UnpairDevices();

        // Accoppia con il dispositivo specifico
        InputUser.PerformPairingWithDevice(device, user);

        // Forza lo schema di controllo
        user.ActivateControlScheme(controlScheme);

        // Configura le impostazioni
        playerInput.neverAutoSwitchControlSchemes = true; // IMPORTANTE: impedisce il cambio automatico
        playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;

        Debug.Log($"Configured PlayerInput with device: {device.name}, scheme: {controlScheme}");
    }
}