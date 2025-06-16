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

                InputManager.Instance.RegisterPlayer(0, CharacterID.CharacterA, g1.GetComponent<PlayerInput>());

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

        // Istanzia usando PlayerInput.Instantiate per maggiore controllo
        PlayerInput player1Input = PlayerInput.Instantiate(
            player1Prefab,
            controlScheme: "Gamepad",
            pairWithDevice: Gamepad.all[pad1Index]
        );

        PlayerInput player2Input = PlayerInput.Instantiate(
            player2Prefab,
            controlScheme: "Gamepad",
            pairWithDevice: Gamepad.all[pad2Index]
        );

        // Posiziona i player
        player1Input.transform.position = player1SpawnPoint.position;
        player1Input.transform.name = "Player1";

        player2Input.transform.position = player2SpawnPoint.position;
        player2Input.transform.name = "Player2";

        // Registra i player
        InputManager.Instance.RegisterLocalPlayer(0, CharacterID.CharacterA, player1Input);
        InputManager.Instance.RegisterLocalPlayer(1, CharacterID.CharacterB, player2Input);

        GameStateManager.Instance.CurrentGameState = GameState.Playing;
    }
}