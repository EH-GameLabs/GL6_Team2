using System;
using System.Collections;
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
    [Header("First Star")]
    public int candiesCollected = 0;
    List<ICollectibles> candies = new List<ICollectibles>();
    [Range(0, 100)] public int candiesPercentageToGetTheStar = 80;

    // STELLA 2
    [Header("Second Star")]
    [SerializeField] float timeMaxToGetTheStar = 180f; // 3 minutes
    private float timeElapsed = 0f;

    // STELLA 3
    [Header("Third Star")]
    public bool hasTakenDamage = false;

    [Header("Scene Settings")]
    public string mainScene;
    public string Level1;
    public string Level2;

    public string currentLevel;


    [ServerRpc(RequireOwnership = false)]
    public void StartLevelServerRpc(string scene)
    {
        // Avvia il livello sul server
        StartLevelClientRpc(scene);
    }

    [ClientRpc]
    private void StartLevelClientRpc(string scene)
    {
        // Avvia il livello su tutti i client
        LoadLevel(scene);
    }

    public void LoadLevel(string scene)
    {
        // carica il livello
        SceneManager.LoadScene(scene, LoadSceneMode.Additive);
        SceneManager.sceneLoaded += OnSceneLoaded;
        currentLevel = scene;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        Debug.Log("Scene loaded: " + arg0.name);

        if (arg0.name == mainScene)
        {
            GameStateManager.Instance.CurrentGameState = GameState.MainMenu;
            return;
        }

        candiesCollected = 0;
        candies.Clear();
        candies.AddRange(FindObjectsByType<Candy>(FindObjectsSortMode.None));

        GameManager.Instance.SpidyLife = GameManager.Instance.spidyMaxLife;
        GameManager.Instance.CandlyLife = GameManager.Instance.candlyMaxLife;

        GameStateManager.Instance.CurrentGameState = GameState.Playing;
        GameManager.Instance.IsGamePaused = false;


        PlayerController[] motors = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (PlayerController motor in motors)
        {
            if (motor.characterId == CharacterID.CharacterA)
            {
                motor.transform.GetComponent<Rigidbody>().useGravity = true;
                break;
            }
        }

        // start timer
        timeElapsed = 0f;
        StartCoroutine(TimerCoroutine());
    }

    private IEnumerator TimerCoroutine()
    {
        while (GameStateManager.Instance.CurrentGameState == GameState.Playing)
        {
            timeElapsed += 1f; // Aggiornare anche UI
            yield return new WaitForSeconds(1f);
        }
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
        AddCandy();
    }

    public void AddCandy()
    {
        candiesCollected++;
        //Debug.Log($"Candy collected! Total: {candiesCollected}/{candies.Count}");
    }

    public float GetCandiesCollected()
    {
        // Controlla se tutti gli oggetti sono stati raccolti
        return (float)candiesCollected / (float)candies.Count;
    }

    internal void EndLevel()
    {
        if (GameManager.Instance.gameMode != GameMode.OnlineMultiplayer)
        {
            LevelEnded();
            return;
        }

        //if (!LobbyManager.Instance.IsHost()) return;
        EndLevelServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void EndLevelServerRpc()
    {
        // Notifica il server che il livello è finito
        LevelEndedClientRpc();
    }

    [ClientRpc]
    private void LevelEndedClientRpc()
    {
        // Gestisce la fine del livello su tutti i client
        LevelEnded();
    }

    private void LevelEnded()
    {
        StopAllCoroutines();

        GameManager.Instance.IsGamePaused = true;

        if (currentLevel == Level1)
        {
            GameStateManager.Instance.CurrentGameState = GameState.Win;
        }
        else if (currentLevel == Level2)
        {
            GameStateManager.Instance.CurrentGameState = GameState.DemoCompleted;
        }

        Debug.Log($"Level ended. Candies collected: {GetCandiesCollected()}, Time elapsed: {timeElapsed}s, Damage taken: {GameManager.Instance.SpidyLife}");

        FindAnyObjectByType<WinUI>(FindObjectsInactive.Include).SetWinStats(
            GetCandiesCollected(),
            timeElapsed, timeMaxToGetTheStar,
            GameManager.Instance.SpidyLife);
    }

    internal string GetCurrentLevel()
    {
        return currentLevel;
    }
}
