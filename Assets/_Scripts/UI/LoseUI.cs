using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoseUI : BaseUI
{
    [SerializeField] private Image imageAnimation;
    [SerializeField] private Sprite sprite1;
    [SerializeField] private Sprite sprite2;

    private void OnEnable()
    {
        StartCoroutine(ShowLoseAnimation());
    }

    private void OnDisable()
    {
        StopCoroutine(ShowLoseAnimation());
        imageAnimation.sprite = sprite1; // Reset to the first sprite when the UI is disabled
    }
    private IEnumerator ShowLoseAnimation()
    {
        Debug.Log("ShowLoseAnimation called");
        while (true)
        {
            yield return new WaitForSecondsRealtime(0.5f);
            Debug.Log("Toggling sprite");
            if (imageAnimation.sprite == sprite1)
            {
                Debug.Log("Switching to sprite2");
                imageAnimation.sprite = sprite2;
            }
            else
            {
                Debug.Log("Switching to sprite1");
                imageAnimation.sprite = sprite1;
            }
        }
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(LevelManager.Instance.mainScene, LoadSceneMode.Single);
    }

    public void GoToLevelSelector()
    {
        if (SceneManager.sceneCount > 1)
        {
            for (int i = 1; i < SceneManager.sceneCount; i++)
            {
                SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(i));
            }
        }
        GameStateManager.Instance.CurrentGameState = GameState.LevelSelector;
    }

    public void RestartLevel()
    {
        if (SceneManager.sceneCount > 1)
        {
            for (int i = 1; i < SceneManager.sceneCount; i++)
            {
                SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(i));
            }
        }
        GameManager.Instance.ResetPlayersPosition();
        LevelManager.Instance.LoadLevel(LevelManager.Instance.GetCurrentLevel());
    }

}
