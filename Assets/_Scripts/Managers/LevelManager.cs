using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
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

    // STELLA 1
    List<ICollectibles> collectibles = new List<ICollectibles>();
    public int collectiblesCollected = 0;

    // STELLA 2
    [SerializeField] float timeMaxToGetTheStar = 180f; // 3 minutes
    private float timeElapsed = 0f;

    // STELLA 3
    public bool hasTakenDamage = false;


    [ServerRpc(RequireOwnership = false)]
    public void StartLevelServerRpc(int level)
    {
        // Avvia il livello sul server
        StartLevelClientRpc(level);
    }

    [ClientRpc]
    private void StartLevelClientRpc(int level)
    {
        // Avvia il livello su tutti i client
        LoadLevel(level);
    }

    public void LoadLevel(int level)
    {
        // carica il livello
        SceneManager.LoadScene("Level" + level, LoadSceneMode.Additive);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        Debug.Log("Scene loaded: " + arg0.name);
        collectibles.AddRange(FindObjectsByType<Sock>(FindObjectsSortMode.None));
        GameStateManager.Instance.CurrentGameState = GameState.Playing;
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddSockServerRpc()
    {
        // Incrementa il conteggio degli oggetti raccolti sul server
        NotifyCollectibleCollectedClientRpc();
    }

    [ClientRpc]
    private void NotifyCollectibleCollectedClientRpc()
    {
        // Notifica tutti i client che un oggetto è stato raccolto
        AddSock();
    }

    public void AddSock()
    {
        collectiblesCollected++;
        Debug.Log($"Sock collected! Total: {collectiblesCollected}/{collectibles.Count}");
    }

    public bool AreAllCollectiblesCollected()
    {
        // Controlla se tutti gli oggetti sono stati raccolti
        return collectibles.Count == collectiblesCollected;
    }
}
