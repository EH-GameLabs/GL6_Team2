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

            // Se il giocatore non è già autenticato, esegui la procedura di sign-in
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                AuthenticationService.Instance.SignedIn += () =>
                {
                    playerId = AuthenticationService.Instance.PlayerId;
                };

                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            else
            {
                // Opzionale: puoi anche assegnare subito il playerId se già autenticato
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
        while (true)
        {
            if (CurrentLobby != null)
            {
                yield return new WaitForSeconds(1.1f);
                UpdateLobbyAsync();
            }
            else
            {
                yield return null;
            }
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

                            NetworkManager.Singleton.OnClientConnectedCallback += (ulong clientId) =>
                            {
                                Debug.Log($"Client {clientId} connected to the host.");
                                if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
                                {
                                    // Chiama la ServerRpc
                                    SerializeGameServerRpc();
                                }
                            };

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
            Debug.LogError("Errore nell'aggiornamento della lobby: " + e.Message);
            if (e.Reason == LobbyExceptionReason.LobbyNotFound || e.Reason == LobbyExceptionReason.Forbidden)
            {
                CurrentLobby = null;
                // Gestisci l'uscita dalla lobby se necessario
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SerializeGameServerRpc()
    {
        SerializeGameClientRpc();
    }

    [ClientRpc]
    public void SerializeGameClientRpc()
    {
        Debug.Log("Game state serialized and sent to clients.");
        GameManager.Instance.SetupGame(GameMode.OnlineMultiplayer);
        GameStateManager.Instance.CurrentGameState = GameState.Playing;
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

        // Se il codice è vuoto, segnala l'errore e interrompi l'operazione
        if (string.IsNullOrEmpty(code))
        {
            Debug.LogError("Inserisci un codice valido per unirti alla lobby.");
            return;
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
            await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, playerId);
            if (NetworkManager.Singleton.isActiveAndEnabled)
                NetworkManager.Singleton.Shutdown();
            CurrentLobby = null;
            GameStateManager.Instance.CurrentGameState = GameState.MainMenu;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Errore nell'uscita dalla lobby: " + e.Message);
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

    public async void StartGame()
    {
        if (!IsHost()) return;
        //if (CurrentLobby.Players.Count != 2)
        //{
        //    Debug.LogWarning("Il gioco può iniziare solo con 2 giocatori.");
        //    return;
        //}

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

            // Aggiorna i dati della lobby per indicare che il gioco è iniziato
            Lobby updatedLobby = await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "IsGameStarted", new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
                }
            });
            CurrentLobby = updatedLobby;

            //GameStateManager.Instance.CurrentGameState = GameState.Playing;

        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Errore nell'avvio del gioco: " + e.Message);
        }
    }

    private new bool IsHost()
    {
        return CurrentLobby != null && CurrentLobby.HostId == playerId;
    }
}
