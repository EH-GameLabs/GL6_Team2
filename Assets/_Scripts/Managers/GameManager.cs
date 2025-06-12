using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Configuration")]
    [SerializeField] private GameMode startingMode;
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
                // Istanzia i due personaggi nella scena
                Instantiate(characterAPrefab, new Vector3(-2, 0, 0), Quaternion.identity);
                Instantiate(characterBPrefab, new Vector3(2, 0, 0), Quaternion.identity);
                GameStateManager.Instance.CurrentGameState = GameState.Playing;
                break;

            case GameMode.LocalMultiplayer:
                // Gestisco con la funzione: "SetupLocalMultiplayerGame"
                break;

            case GameMode.OnlineMultiplayer:
                Instantiate(characterAPrefab, new Vector3(-2, 0, 0), Quaternion.identity);
                Instantiate(characterBPrefab, new Vector3(2, 0, 0), Quaternion.identity);

                // La logica di setup online è gestita da Netcode, Lobby, etc.
                // Il GameManager qui potrebbe solo settare la modalità.
                // Lo spawn e la registrazione verranno gestiti da un altro script (vedi sotto).
                break;
        }
    }

    public void SetupLocalMultiplayerGame(int pad1Index, int pad2Index)
    {
        InputManager.Instance.SetGameMode(GameMode.LocalMultiplayer);

        var pim = gameObject.GetComponent<PlayerInputManager>();
        // Non assegnare playerPrefab qui se non vuoi che PlayerInputManager spawni qualcosa
        // pim.playerPrefab = playerControllerPrefab; // Rimuovi o commenta

        // Istanzia i due personaggi (mantieni il tuo codice originale)
        GameObject charA = Instantiate(characterAPrefab, new Vector3(-2, 0, 0), Quaternion.identity);
        GameObject charB = Instantiate(characterBPrefab, new Vector3(2, 0, 0), Quaternion.identity);

        // Crea PlayerInput per il primo giocatore
        PlayerInput player1Input = PlayerInput.Instantiate(playerControllerPrefab, controlScheme: "Gamepad", pairWithDevice: Gamepad.all[pad1Index]);
        InputManager.Instance.RegisterPlayer(0, CharacterID.CharacterA, player1Input);

        // Crea PlayerInput per il secondo giocatore
        PlayerInput player2Input = PlayerInput.Instantiate(playerControllerPrefab, controlScheme: "Gamepad", pairWithDevice: Gamepad.all[pad2Index]);
        InputManager.Instance.RegisterPlayer(1, CharacterID.CharacterB, player2Input);

        GameStateManager.Instance.CurrentGameState = GameState.Playing;
    }
}