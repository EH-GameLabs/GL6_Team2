using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseUI : BaseUI
{

    public void GoToMainMenu()
    {
        if (GameManager.Instance.gameMode == GameMode.OnlineMultiplayer)
        {
            NetworkManager.Singleton.Shutdown();
        }
        SceneManager.LoadScene("MainScene", LoadSceneMode.Single);

        SceneManager.sceneLoaded += (scene, mode) =>
        {
            if (scene.name == "MainScene")
            {
                GameStateManager.Instance.CurrentGameState = GameState.MainMenu;
            }
        };
    }

    public void GoToHud()
    {
        UIManager.Instance.ShowUI(GameState.Playing);
    }
}
