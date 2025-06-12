using UnityEngine;
using UnityEngine.InputSystem;

public class GamepadUIController : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;

    [Header("Debugging variables")]
    [SerializeField][ReadOnly] private Vector2 navigationInput;

    private LocalMultiplayerUI localMultiplayerUI;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        localMultiplayerUI = FindAnyObjectByType<LocalMultiplayerUI>(FindObjectsInactive.Include);
    }

    private void OnNavigate(InputValue value)
    {
        navigationInput = value.Get<Vector2>();
        if (navigationInput.x == -1)
        {
            Debug.Log("Navigating Left");
            localMultiplayerUI.NavigatePlayerToDir(playerInput.playerIndex, false);
        }
        else if (navigationInput.x == 1)
        {
            Debug.Log("Navigating Right");
            localMultiplayerUI.NavigatePlayerToDir(playerInput.playerIndex, true);
        }
    }
}
