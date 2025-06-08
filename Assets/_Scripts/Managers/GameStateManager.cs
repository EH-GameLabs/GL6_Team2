using System;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    [SerializeField][ReadOnly] private GameState _currentGameState;
    public GameState CurrentGameState
    {
        get => _currentGameState;
        set
        {
            if (_currentGameState != value)
            {
                _currentGameState = value;
                OnGameStateChanged(value);
            }
        }
    }

    private void OnGameStateChanged(GameState newState)
    {
        Debug.Log($"Game state changed to: {newState}");
        UIManager.Instance.ShowUI(newState);
    }
}
