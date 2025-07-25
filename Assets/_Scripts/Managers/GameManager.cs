using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public int spidyMaxLife = 3;
    [ReadOnly] private int spidyLife = 3;
    public int SpidyLife
    {
        get => spidyLife;
        set
        {
            spidyLife = Mathf.Clamp(value, 0, spidyMaxLife);
            FindAnyObjectByType<HudUI>(FindObjectsInactive.Include).UpdateSpidyLife(spidyLife);
        }
    }

    public int candlyMaxLife = 1;
    [ReadOnly] private int candlyLife = 1;
    public int CandlyLife
    {
        get => candlyLife;
        set
        {
            candlyLife = Mathf.Clamp(value, 0, candlyMaxLife);
            //FindAnyObjectByType<HudUI>(FindObjectsInactive.Include).UpdateCandlyLife(candlyLife);
        }
    }

    [Header("GameObjects Prefabs")]
    public GameObject player1Prefab;
    public GameObject player2Prefab;
    public Transform player1SpawnPoint;
    public Transform player2SpawnPoint;

    [Header("Configuration")]
    [ReadOnly] public GameMode gameMode;

    private bool _isGamePaused = true;
    public bool IsGamePaused
    {
        get => _isGamePaused;
        set
        {
            if (value)
            {
                Time.timeScale = 0f; // Ferma il tempo
            }
            else
            {
                Time.timeScale = 1f; // Riprende il tempo
            }
            _isGamePaused = value;
        }
    }

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

        IsGamePaused = true;
    }

    public void SetupGame(GameMode mode)
    {
        switch (mode)
        {
            case GameMode.SinglePlayer:
                GameObject p1 = Instantiate(player1Prefab, player1SpawnPoint.position, Quaternion.identity);
                p1.name = "Player1";
                GameObject p2 = Instantiate(player2Prefab, player2SpawnPoint.position, Quaternion.identity);
                p2.name = "Player2";

                FindAnyObjectByType<CameraFollow>().SetTarget(p1.transform);

                PlayerInput playerInput = p1.GetComponent<PlayerInput>();
                playerInput.enabled = true;

                InputManager.Instance.RegisterPlayer(0, CharacterID.CharacterA, playerInput);
                //GameStateManager.Instance.CurrentGameState = GameState.Playing;
                break;

            case GameMode.LocalMultiplayer:
                // Gestisco con la funzione: "SetupLocalMultiplayerGame"
                break;

            case GameMode.OnlineMultiplayer:

                break;
        }

        SpidyLife = spidyMaxLife;
        candlyLife = candlyMaxLife;
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
        //p1.transform.position.Set(player1SpawnPoint.position.x, player1SpawnPoint.position.y, 0);
        GameObject p2 = Instantiate(player2Prefab, player2SpawnPoint.position, Quaternion.identity);
        p2.transform.position.Set(player2SpawnPoint.position.x, player2SpawnPoint.position.y, 0);

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

        FindAnyObjectByType<CameraFollow>().SetTarget(p1.transform);

        ConfigurePlayerInput(player1Input, Gamepad.all[pad1Index], "Gamepad");
        ConfigurePlayerInput(player2Input, Gamepad.all[pad2Index], "Gamepad");

        // Registra i player
        InputManager.Instance.RegisterLocalPlayer(0, CharacterID.CharacterA, player1Input);
        InputManager.Instance.RegisterLocalPlayer(1, CharacterID.CharacterB, player2Input);

        //GameStateManager.Instance.CurrentGameState = GameState.Playing;
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

    internal void AddSock()
    {
        throw new NotImplementedException();
    }

    public void LoseLife(CharacterID characterID)
    {
        switch (characterID)
        {
            case CharacterID.CharacterA:
                SpidyLife--;

                SoundManager.Instance.PlaySFXSound(SoundManager.Instance.LifeLost);
                LevelManager.Instance.hasTakenDamage = true;
                if (SpidyLife <= 0)
                {
                    Debug.Log("Spidy has lost all lives!");

                    IsGamePaused = true;
                    GameStateManager.Instance.CurrentGameState = GameState.Lose;
                }
                break;
            case CharacterID.CharacterB:
                candlyLife--;
                if (candlyLife <= 0)
                {
                    Debug.Log("Candly has lost all lives!");
                    // Gestisci la morte di Candly

                    IsGamePaused = true;
                    GameStateManager.Instance.CurrentGameState = GameState.Lose;
                }
                break;
            default:
                Debug.LogWarning("Unknown character ID: " + characterID);
                break;
        }
    }

    internal void ResetPlayersPosition()
    {
        PlayerController[] gameObjects = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (PlayerController player in gameObjects)
        {
            if (player.characterId == CharacterID.CharacterA)
            {
                player.transform.position = player1SpawnPoint.position;
            }
            else if (player.characterId == CharacterID.CharacterB)
            {
                player.transform.position = player2SpawnPoint.position;
            }
        }
    }
}