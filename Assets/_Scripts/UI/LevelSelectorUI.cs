using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectorUI : BaseUI
{
    string levelString = "";

    public void StartLevel(int levelIndex)
    {
        Debug.Log("Starting level: " + levelIndex);
        Debug.Log("Is Host: " + LobbyManager.Instance.IsHost());


        if (GameManager.Instance.gameMode != GameMode.OnlineMultiplayer)
        {
            LoadLevel(levelIndex);
        }
        else if (LobbyManager.Instance.IsHost())
        {
            LobbyManager.Instance.StartGame();

            SetLevelClientRpc(levelIndex);
        }
    }

    [ClientRpc]
    private void SetLevelClientRpc(int levelIndex)
    {
        Debug.Log("Setting level on client: " + levelIndex);
        LobbyManager.Instance.levelToStart = levelIndex;
    }

    public void LoadLevel(int levelIndex)
    {
        switch (levelIndex)
        {
            case 1:
                levelString = LevelManager.Instance.Level1;
                break;
            case 2:
                levelString = LevelManager.Instance.Level2;
                break;
            default:
                Debug.LogError("Invalid level index: " + levelIndex);
                return;
        }

        // se ho già scene caricate, le rimuovo
        if (SceneManager.sceneCount > 1)
        {
            for (int i = 1; i < SceneManager.sceneCount; i++)
            {
                SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(i));
            }
        }
        LevelManager.Instance.LoadLevel(levelString);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(LevelManager.Instance.mainScene, LoadSceneMode.Single);
    }
}
