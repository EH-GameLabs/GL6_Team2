using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class DropSpawner : MonoBehaviour
{
    [SerializeField] private float spawnInterval = 2f; // Intervallo di spawn in secondi
    [SerializeField] private GameObject dropPrefab; // Prefab della goccia d'acqua

    GameMode gameMode;

    private void Start()
    {
        gameMode = GameManager.Instance.gameMode;

        if (gameMode == GameMode.OnlineMultiplayer && !NetworkManager.Singleton.IsServer) return;
        StartCoroutine(SpawnDrops());
    }

    private IEnumerator SpawnDrops()
    {
        // sono server, quindi inizio a spawnare le gocce d'acqua
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            // Se il gioco è in modalità online multiplayer, chiama il metodo ClientRpc per spawnare la goccia
            if (gameMode == GameMode.OnlineMultiplayer)
            {
                SpawnDropClientRpc();
            }
            else
            {
                // Se il gioco è in modalità offline, spawn direttamente la goccia
                SpawnDrop();
            }
        }
    }


    [ClientRpc]
    private void SpawnDropClientRpc()
    {
        GameObject g = SpawnDrop();
        g.GetComponent<NetworkObject>().Spawn(); // Assicura che l'oggetto sia sincronizzato tra i client
    }

    private GameObject SpawnDrop()
    {
        // Istanzia una goccia d'acqua alla posizione del DropSpawner
        return Instantiate(dropPrefab, transform.position, Quaternion.identity);
    }
}
