using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab; //add prefab in inspector

    [ServerRpc(RequireOwnership = false)] //server owns this object but client can request a spawn
    public void SpawnPlayerServerRpc(ulong clientId)
    {
        GameObject newPlayer;

        newPlayer = Instantiate(playerPrefab);

        NetworkObject netObj = newPlayer.GetComponent<NetworkObject>();
        newPlayer.SetActive(true);
        netObj.SpawnAsPlayerObject(clientId, true);
    }

    public override void OnNetworkSpawn()
    {
        SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId);
    }
}
