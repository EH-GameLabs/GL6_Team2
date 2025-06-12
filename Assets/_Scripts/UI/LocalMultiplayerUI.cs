using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LocalMultiplayerUI : BaseUI
{
    [Header("Local Multiplayer UI Elements")]
    [SerializeField] private GameObject padUIPrefab1;
    [SerializeField] private GameObject padUIPrefab2;

    [SerializeField] private GameObject StartButton;

    public int player1Index;
    public int player2Index;

    private void Start()
    {
        player1Index = -1;
        player2Index = -1;

        UpdatePlayerPads();
    }

    private void Update()
    {
        UpdatePlayerPads();
    }

    private void UpdatePlayerPads()
    {
        padUIPrefab1.SetActive(false);
        padUIPrefab2.SetActive(false);

        if (Gamepad.all.Count < 1)
        {
            StartButton.SetActive(false);
            return;
        }

        padUIPrefab1.SetActive(Gamepad.all[0] != null);
        padUIPrefab2.SetActive(Gamepad.all.Count > 1 && Gamepad.all[1] != null);

        if (padUIPrefab1.activeInHierarchy &&
            padUIPrefab2.activeInHierarchy &&
            ((player1Index == 0 && player2Index == 1) ||
             (player1Index == 1 && player2Index == 0)))
        {
            StartButton.SetActive(true);
        }
        else
        {
            StartButton.SetActive(false);
        }
    }

    public void StartGame()
    {
        GameStateManager.Instance.CurrentGameState = GameState.Loading;
        GameManager.Instance.SetupLocalMultiplayerGame(player1Index, player2Index);
    }

    public void GoBack()
    {
        GameStateManager.Instance.CurrentGameState = GameState.MainMenu;
    }

    public void NavigatePlayerToDir(int index, bool right)
    {
        if (index == 0)
        {
            player1Index = right ? 1 : 0;
            padUIPrefab1.transform.localPosition = right ?
                new Vector3(300, -50, transform.localPosition.z) :
                new Vector3(-300, -50, transform.localPosition.z);
            Debug.Log($"Player 1 navigated to index: {player1Index}");
        }
        else if (index == 1)
        {
            player2Index = right ? 1 : 0;
            padUIPrefab2.transform.localPosition = right ?
                new Vector3(300, -200, transform.localPosition.z) :
                new Vector3(-300, -200, transform.localPosition.z);
            Debug.Log($"Player 2 navigated to index: {player2Index}");
        }
    }
}
