using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpawner : NetworkBehaviour
{

    [ServerRpc(RequireOwnership = false)]
    public void SpawnPlayerServerRpc(ulong clientId)
    {
        GameObject playerPrefab;
        Transform spawnPoint;

        // Determina quale prefab usare basandosi sull'ID del client
        // Client 0 = Player 1, Client 1 = Player 2
        if (clientId == 0)
        {
            playerPrefab = GameManager.Instance.player1Prefab;
            spawnPoint = GameManager.Instance.player1SpawnPoint;
        }
        else
        {
            playerPrefab = GameManager.Instance.player2Prefab;
            spawnPoint = GameManager.Instance.player2SpawnPoint;
        }

        PlayerInput newPlayer = PlayerInput.Instantiate(
            playerPrefab,
            controlScheme: "Keyboard&Mouse",
            pairWithDevices: new InputDevice[] { Keyboard.current, Mouse.current }
        );

        newPlayer.transform.position = spawnPoint.position;


        NetworkObject netObj = newPlayer.GetComponent<NetworkObject>();
        //newPlayer.SetActive(true);
        netObj.SpawnAsPlayerObject(clientId, true);

        netObj.GetComponent<Rigidbody>().useGravity = false;
        Debug.Log($"Spawned player prefab for client {clientId}");
    }

    public override void OnNetworkSpawn()
    {
        SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId);
    }
}
