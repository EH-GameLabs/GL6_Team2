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
        SceneManager.LoadScene(0);
    }

    public void GoToHud()
    {
        UIManager.Instance.ShowUI(GameState.Playing);
    }
}
