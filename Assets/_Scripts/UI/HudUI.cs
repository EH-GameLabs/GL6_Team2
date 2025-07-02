using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HudUI : BaseUI
{
    [Header("Player Life")]
    [SerializeField] private List<GameObject> playerLifes = new();
    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite emptyHeartSprite;


    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            GameStateManager.Instance.CurrentGameState = GameState.Paused;

            if (GameManager.Instance.gameMode != GameMode.OnlineMultiplayer)
            {
                GameManager.Instance.IsGamePaused = true;
            }
            else
            {
                // TODO: gestire online pause
            }
        }
    }

    public void UpdateSpidyLife(int life)
    {
        Debug.Log($"Update Spidy Life: {life}");
        for (int i = 0; i < playerLifes.Count; i++)
        {
            if (i < life)
            {
                playerLifes[i].GetComponent<Image>().sprite = fullHeartSprite;
            }
            else
            {
                playerLifes[i].GetComponent<Image>().sprite = emptyHeartSprite;
            }
        }
    }
}
