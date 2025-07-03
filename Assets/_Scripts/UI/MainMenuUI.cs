using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainMenuUI : BaseUI
{

    public void StartSinglePlayer()
    {
        GameManager.Instance.gameMode = GameMode.SinglePlayer;
        GameStateManager.Instance.CurrentGameState = GameState.Loading;
        GameManager.Instance.SetupGame(GameMode.SinglePlayer);

        // DEBUG
        GameStateManager.Instance.CurrentGameState = GameState.LevelSelector;
    }

    public void StartLocalMultiplayer()
    {
        GameManager.Instance.gameMode = GameMode.LocalMultiplayer;
        GameStateManager.Instance.CurrentGameState = GameState.LocalMultiplayer;
    }

    public void CreateLobby()
    {
        GameManager.Instance.gameMode = GameMode.OnlineMultiplayer;
        GameStateManager.Instance.CurrentGameState = GameState.Loading;
        LobbyManager.Instance.CreateLobby();
    }

    public void JoinLobby()
    {
        GameManager.Instance.gameMode = GameMode.OnlineMultiplayer;
        GameStateManager.Instance.CurrentGameState = GameState.Join;
    }

    public void GoToOptions()
    {
        GameStateManager.Instance.CurrentGameState = GameState.Options;
    }
}
