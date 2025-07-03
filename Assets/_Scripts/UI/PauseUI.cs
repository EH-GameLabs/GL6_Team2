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

        SceneManager.LoadScene(LevelManager.Instance.mainScene, LoadSceneMode.Single);
    }

    public void GoToHud()
    {
        GameManager.Instance.IsGamePaused = false;
        GameStateManager.Instance.CurrentGameState = GameState.Playing;
    }

    public void ExitLevel()
    {
        if (SceneManager.sceneCount > 1)
        {
            for (int i = 1; i < SceneManager.sceneCount; i++)
            {
                SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(i));
            }
        }
        GameStateManager.Instance.CurrentGameState = GameState.LevelSelector;
    }
}
