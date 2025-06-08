using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseUI : MonoBehaviour, IGameUI
{
    [SerializeField] private GameState gameUI;

    public GameState GetUIType()
    {
        return gameUI;
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
