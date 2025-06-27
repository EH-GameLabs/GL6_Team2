using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinUI : BaseUI
{
    public void GoToMainMenu()
    {
        SceneManager.LoadScene(LevelManager.Instance.mainScene, LoadSceneMode.Single);
    }

    internal void SetWinStats(bool allCandiesCollected, bool lowTime, bool hasTakenDamage)
    {

    }
}
