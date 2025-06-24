using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinUI : BaseUI
{
    public void GoToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    internal void SetWinStats(bool allCandiesCollected, bool lowTime, bool hasTakenDamage)
    {

    }
}
