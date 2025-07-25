using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance;

    [Header("Create Room Panel")]
    [SerializeField] private int maxPlayers = 5;

    private string playerId;
    public static Lobby CurrentLobby { get; private set; }

    private RoomUI roomUI;
    public int levelToStart;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        roomUI = FindAnyObjectByType<RoomUI>(FindObjectsInactive.Include);
        NetworkingInitialize();
    }

    private async void NetworkingInitialize()
    {
        try
        {
            await UnityServices.InitializeAsync();

            // Se il giocatore non � gi� autenticato, esegui la procedura di sign-in
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                AuthenticationService.Instance.SignedIn += () =>
                {
                    playerId = AuthenticationService.Instance.PlayerId;
                };

                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("Player signed in with ID: " + AuthenticationService.Instance.PlayerId);
            }
            else
            {
                // Opzionale: puoi anche assegnare subito il playerId se gi� autenticato
                playerId = AuthenticationService.Instance.PlayerId;
            }

            // Avvia la coroutine per aggiornare periodicamente la lobby
            StartCoroutine(LobbyUpdateRoutine());
        }
        catch (Exception ex)
        {
            Debug.LogError("Errore durante l'inizializzazione: " + ex.Message);
        }
    }

    // Coroutine che aggiorna lo stato della lobby ogni 1.1 secondi
    private IEnumerator LobbyUpdateRoutine()
    {
        const float UPDATE_INTERVAL = 1.1f;

        while (true)
        {
            if (CurrentLobby != null)
            {
                Debug.Log("Updating lobby: " + CurrentLobby.Id);
                UpdateLobbyAsync();
            }
            else
            {
                Debug.Log("No current lobby to update.");
            }

            yield return new WaitForSecondsRealtime(UPDATE_INTERVAL);
        }
    }

    private async void UpdateLobbyAsync()
    {
        int retryCount = 0;
        int maxRetries = 10;

        try
        {
            if (IsInLobby())
            {
                // Ottieni i dati della lobby
                CurrentLobby = await LobbyService.Instance.GetLobbyAsync(CurrentLobby.Id);
                VisualizeRoomDetails();

                // Loop di retry fino a che "IsGameStarted" non diventi diverso da "0"
                while (retryCount < maxRetries)
                {
                    if (CurrentLobby.Data.TryGetValue("IsGameStarted", out DataObject isGameStartedData) && isGameStartedData.Value != "0")
                    {
                        if (!IsHost())
                        {
                            GameStateManager.Instance.CurrentGameState = GameState.Loading;
                            string relayJoinCode = isGameStartedData.Value;
                            Debug.Log("RelayJoinCode (client): " + relayJoinCode);

                            // Unisciti alla Relay allocation
                            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
                            //NEW
                            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(joinAllocation, "dtls"));

                            // OLD
                            //RelayServerData relayServerData = new AllocationUtils.ToRelayServerData(hostAllocation, connectionType);
                            //UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                            //transport.SetRelayServerData(relayServerData);

                            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

                            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
                            {
                                if (!NetworkManager.Singleton.StartClient())
                                {
                                    Debug.LogError("Impossibile avviare il client.");
                                }
                                else
                                {
                                    StopAllCoroutines();
                                    return;
                                }
                            }
                        }
                        break;
                    }
                    else
                    {
                        if (IsHost()) return;

                        // Non ancora pronto: attendi e riprova
                        retryCount++;
                        Debug.Log($"Lobby not updated yet. Retry {retryCount}/{maxRetries}...");
                        //await Task.Delay((int)(retryDelay * 1000));
                        // Ricarica la lobby
                        CurrentLobby = await LobbyService.Instance.GetLobbyAsync(CurrentLobby.Id);
                        VisualizeRoomDetails();
                    }
                }

                if (retryCount >= maxRetries)
                {
                    //Debug.Log("Retry limit reached. The game did not start.");
                }
            }
        }
        catch (LobbyServiceException e)
        {
            //Debug.LogError("Errore nell'aggiornamento della lobby: " + e.Message);
            if (e.Reason == LobbyExceptionReason.LobbyNotFound || e.Reason == LobbyExceptionReason.Forbidden)
            {
                CurrentLobby = null;
                // Gestisci l'uscita dalla lobby se necessario
            }
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} connected to the host.");

        // Questo codice viene eseguito sia sull'host che sui client
        if (NetworkManager.Singleton.IsHost)
        {
            // Codice che viene eseguito solo sull'host
            Debug.Log($"Host: Client {clientId} si � connesso");

            // Invia il livello al client appena connesso
            SetLevelClientRpc(levelToStart);
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            // Codice che viene eseguito solo sui client (non host)
            Debug.Log($"Client: Altro client {clientId} si � connesso");

            // Se questo client si � appena connesso, richiedi il livello all'host
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                RequestLevelFromHostServerRpc();
            }
        }
    }

    // Metodo per richiedere il livello all'host
    [ServerRpc(RequireOwnership = false)]
    public void RequestLevelFromHostServerRpc(ServerRpcParams rpcParams = default)
    {
        // Ottieni l'ID del client che ha fatto la richiesta
        ulong clientId = rpcParams.Receive.SenderClientId;

        Debug.Log($"Client {clientId} ha richiesto il livello corrente");

        // Invia il livello al client specifico
        SetLevelClientRpc(levelToStart);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SerializeGameServerRpc()
    {
        SerializeGameClientRpc();
    }

    [ClientRpc]
    private void SetLevelClientRpc(int levelIndex)
    {
        Debug.Log("Setting level on client: " + levelIndex);
        levelToStart = levelIndex;
        SerializeGameClientRpc();
    }

    [ClientRpc]
    public void SerializeGameClientRpc()
    {
        Debug.Log("Game state serialized and sent to clients.");
        GameManager.Instance.SetupGame(GameMode.OnlineMultiplayer);

        PlayerController[] Characters = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (PlayerController character in Characters)
        {
            if (character.characterId == CharacterID.CharacterA)
            {
                FindAnyObjectByType<CameraFollow>().SetTarget(character.transform);
                break;
            }
        }

        FindAnyObjectByType<LevelSelectorUI>(FindObjectsInactive.Include).LoadLevel(levelToStart);
    }

    public async void CreateLobby()
    {
        try
        {
            string lobbyName = "New Lobby" + UnityEngine.Random.Range(0, 99999);
            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = true,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    {"IsGameStarted", new DataObject(DataObject.VisibilityOptions.Member, "0")}
                }
            };

            CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

            Player player = CurrentLobby.Players.Find(p => p.Id == playerId);
            roomUI.UpdatePlayerList(player.Data["PlayerName"].Value, "", IsHost());

            EnterRoom();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Errore nella creazione della lobby: " + e.Message);
        }
    }

    private void EnterRoom()
    {
        GameStateManager.Instance.CurrentGameState = GameState.Room;

        roomUI.roomCodeText.text = "RoomCode: " + CurrentLobby.LobbyCode;

        VisualizeRoomDetails();
    }

    private void VisualizeRoomDetails()
    {
        List<string> playerNames = new List<string>();

        if (IsInLobby())
        {
            foreach (Player player in CurrentLobby.Players)
            {
                if (player.Data != null && player.Data.ContainsKey("PlayerName"))
                {
                    playerNames.Add(player.Data["PlayerName"].Value);
                }
                else
                {
                    playerNames.Add("Unknown");
                }
            }

            roomUI.startGameButton.SetActive(IsHost());

            roomUI.UpdatePlayerList(playerNames[0], CurrentLobby.Players.Count < 2 ? "..." : playerNames[1], IsHost());
        }
    }

    private bool IsInLobby()
    {
        if (CurrentLobby == null) return false;
        foreach (Player player in CurrentLobby.Players)
        {
            if (player.Id == playerId)
            {
                return true;
            }
        }
        Debug.LogWarning("Giocatore non trovato nella lobby. Uscita dalla lobby.");
        CurrentLobby = null;
        return false;
    }

    public async void JoinLobbyWithCode(string roomCodeInput)
    {
        string code = roomCodeInput.Trim();

        // Se il codice � vuoto, segnala l'errore e interrompi l'operazione
        if (string.IsNullOrEmpty(code))
        {
            Debug.LogError("Inserisci un codice valido per unirti alla lobby.");
            return;
        }

        if (CurrentLobby != null)
        {
            await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, playerId);
            CurrentLobby = null;
        }

        try
        {
            JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };
            CurrentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code, options);
            if (CurrentLobby != null)
            {
                EnterRoom();
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Errore nel join della lobby: " + e.Message);
        }
    }


    public async void LeaveRoom()
    {
        if (CurrentLobby == null)
            return;

        try
        {
            // Rimuovi il player dalla lobby
            await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, playerId);

            // Shutdown del NetworkManager
            if (NetworkManager.Singleton.isActiveAndEnabled)
            {
                NetworkManager.Singleton.Shutdown();
            }

            // Cleanup immediato
            CurrentLobby = null;
            GameStateManager.Instance.CurrentGameState = GameState.MainMenu;

            Debug.Log("Left room successfully");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Errore nell'uscita dalla lobby: " + e.Message);

            // Cleanup anche in caso di errore
            CurrentLobby = null;
            GameStateManager.Instance.CurrentGameState = GameState.MainMenu;
        }
    }

    private async void KickPlayer(string targetPlayerId)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, targetPlayerId);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Errore nel kick del giocatore: " + e.Message);
        }
    }

    private Player GetPlayer()
    {
        string name = PlayerPrefs.GetString("Name");
        if (string.IsNullOrEmpty(name))
            name = playerId;
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, name) }
            }
        };
    }

    public bool CanStartGame()
    {
        if (CurrentLobby.Players.Count == 2)
        {
            return true;
        }
        Debug.LogWarning("Il gioco pu� iniziare solo con 2 giocatori.");
        return false;
    }

    public async void StartGame()
    {
        if (!IsHost()) return;

        GameStateManager.Instance.CurrentGameState = GameState.Loading;
        try
        {
            Debug.Log("Avvio del gioco...");
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            // NEW
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));

            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(relayJoinCode);

            // OLD
            //RelayServerData relayServerData = new RelayServerData();
            //UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            //transport.SetRelayServerData(relayServerData);

            bool hostStarted = NetworkManager.Singleton.StartHost();
            if (!hostStarted)
            {
                Debug.LogError("Impossibile avviare l'host.");
                return;
            }

            // Aggiorna i dati della lobby per indicare che il gioco � iniziato
            Lobby updatedLobby = await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "IsGameStarted", new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
                }
            });
            CurrentLobby = updatedLobby;
            Debug.Log("Game started with Relay Join Code: " + relayJoinCode);

            //GameStateManager.Instance.CurrentGameState = GameState.Playing;

        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Errore nell'avvio del gioco: " + e.Message);
        }
    }

    public new bool IsHost()
    {
        return CurrentLobby != null && CurrentLobby.HostId == playerId;
    }
}
