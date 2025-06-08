using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private Dictionary<GameState, IGameUI> registeredUIs = new Dictionary<GameState, IGameUI>();
    public Transform UIContainer;
    private GameState currentActiveUI = GameState.NONE;
    public GameState startingGameUI;

    public void RegisterUI(GameState uiType, IGameUI uiToRegister)
    {
        registeredUIs.Add(uiType, uiToRegister);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;

        foreach (IGameUI enumeratedUI in UIContainer.GetComponentsInChildren<IGameUI>(true))
        {
            RegisterUI(enumeratedUI.GetUIType(), enumeratedUI);
        }

        ShowUI(startingGameUI); // TODO -> SHOW MAIN MENU FIRST
    }

    public void ShowUI(GameState uiType)
    {
        foreach (KeyValuePair<GameState, IGameUI> kvp in registeredUIs)
        {
            kvp.Value.SetActive(kvp.Key == uiType);
        }

        currentActiveUI = uiType;
    }

    public GameState GetCurrentActiveUI()
    {
        return currentActiveUI;
    }
}