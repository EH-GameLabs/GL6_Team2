using TMPro;
using UnityEngine;

public class JoinUI : BaseUI
{
    [Header("Join UI Elements")]
    [SerializeField] private TMP_InputField lobbyCodeInputField;

    public void JoinLobby()
    {
        GameStateManager.Instance.CurrentGameState = GameState.Loading;
        LobbyManager.Instance.JoinLobbyWithCode(lobbyCodeInputField.text);
    }

    public void GoBack()
    {
        GameStateManager.Instance.CurrentGameState = GameState.MainMenu;
    }
}
