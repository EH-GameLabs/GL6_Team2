using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainMenuUI : BaseUI
{
    [Header("Main Menu UI Elements")]
    [SerializeField] private TMP_InputField playerNameInputField;

    private void Start()
    {
        playerNameInputField.onValueChanged.AddListener(OnPlayerNameChanged);
        playerNameInputField.text = PlayerPrefs.GetString("Name", "Player");
    }

    private void OnPlayerNameChanged(string newName)
    {
        PlayerPrefs.SetString("Name", newName);
    }

    public void StartSinglePlayer()
    {
        GameStateManager.Instance.CurrentGameState = GameState.Loading;
        GameManager.Instance.SetupGame(GameMode.SinglePlayer);
    }

    public void StartLocalMultiplayer()
    {
        GameStateManager.Instance.CurrentGameState = GameState.LocalMultiplayer;
    }

    public void CreateLobby()
    {
        GameStateManager.Instance.CurrentGameState = GameState.Loading;
        LobbyManager.Instance.CreateLobby();
    }

    public void JoinLobby()
    {
        GameStateManager.Instance.CurrentGameState = GameState.Join;
    }

    public void GoToOptions()
    {
        GameStateManager.Instance.CurrentGameState = GameState.Options;
    }
}
