using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HudUI : BaseUI
{

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            GameStateManager.Instance.CurrentGameState = GameState.Paused;
        }
    }
}
