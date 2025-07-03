using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomUI : BaseUI
{
    [Header("Room UI Elements")]
    [SerializeField] private TextMeshProUGUI player1Text;
    [SerializeField] private TextMeshProUGUI player2Text;
    [SerializeField] public TextMeshProUGUI roomCodeText;

    [SerializeField] public GameObject startGameButton;

    [Header("JOIN UI")]
    [SerializeField] private TMP_InputField roomCodeInputField;

    public void UpdatePlayerList(string player1, string player2, bool isHost)
    {
        player1Text.text = player1;
        player2Text.text = player2;

        if (isHost)
        {
            player1Text.color = Color.blue;
            player1Text.fontStyle = FontStyles.Bold;

            player2Text.color = Color.black;
        }
        else
        {
            player1Text.color = Color.black;

            player2Text.color = Color.blue;
            player2Text.fontStyle = FontStyles.Bold;
        }
    }

    public void StartGame()
    {
        LobbyManager.Instance.StartGame();
    }

    public void LeaveGame()
    {
        GameStateManager.Instance.CurrentGameState = GameState.Loading;
        LobbyManager.Instance.LeaveRoom();
    }

    public void CopyRoomCode()
    {
        GUIUtility.systemCopyBuffer = roomCodeText.text.Split(' ')[1];
        Debug.Log("Room code copied to clipboard: " + roomCodeText.text.Split(' ')[1]);
    }

    // JOIN
    public void JoinRoom()
    {
        string roomCode = roomCodeInputField.text.Trim();
        if (!string.IsNullOrEmpty(roomCode))
        {
            LobbyManager.Instance.JoinLobbyWithCode(roomCode);
        }
        else
        {
            Debug.LogWarning("Room code cannot be empty!");
        }
    }
}
