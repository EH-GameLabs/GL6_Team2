using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinUI : BaseUI
{
    [Header("First Star")]
    [SerializeField] private TextMeshProUGUI candiesCollectedPercentage;
    [SerializeField] private GameObject completed;
    [SerializeField] private GameObject notCompleted;

    [Header("Second Star")]
    [SerializeField] private TextMeshProUGUI timeElapsedText;
    [SerializeField] private GameObject timeCompleted;
    [SerializeField] private GameObject timeNotCompleted;

    [Header("Third Star")]
    [SerializeField] private List<GameObject> damageTakenText;
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite emptyHeart;
    [SerializeField] private GameObject damageTakenCompleted;
    [SerializeField] private GameObject damageTakenNotCompleted;

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(LevelManager.Instance.mainScene, LoadSceneMode.Single);
    }

    internal void SetWinStats(float allCandiesCollected, float timeElapsed, float maxTime, int spidyLife)
    {
        SetCandiesPercentage(allCandiesCollected);
        SetTimeElapsed(timeElapsed, maxTime);
        SetDamageTaken(spidyLife);
    }

    private void SetCandiesPercentage(float percentage)
    {
        // Calcola la percentuale di caramelle raccolte --> 91%
        candiesCollectedPercentage.text = $"{Mathf.RoundToInt(percentage * 100)}%";
        if (percentage >= (LevelManager.Instance.candiesPercentageToGetTheStar * 0.01f))
        {
            completed.SetActive(true);
            notCompleted.SetActive(false);
        }
        else
        {
            completed.SetActive(false);
            notCompleted.SetActive(true);
        }
    }

    private void SetTimeElapsed(float timeElapsed, float maxTime)
    {
        // tempo espresso in min:sec --> 00:00
        TimeSpan timeSpan = TimeSpan.FromSeconds(timeElapsed);
        timeElapsedText.text = $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        if (timeElapsed <= maxTime)
        {
            timeCompleted.SetActive(true);
            timeNotCompleted.SetActive(false);
        }
        else
        {
            timeCompleted.SetActive(false);
            timeNotCompleted.SetActive(true);
        }
    }

    internal void SetDamageTaken(int spidyLife)
    {
        for (int i = 0; i < damageTakenText.Count; i++)
        {
            if (i < spidyLife)
            {
                damageTakenText[i].GetComponent<UnityEngine.UI.Image>().sprite = fullHeart;
            }
            else
            {
                damageTakenText[i].GetComponent<UnityEngine.UI.Image>().sprite = emptyHeart;
            }
        }
        if (spidyLife == GameManager.Instance.spidyMaxLife)
        {
            damageTakenCompleted.SetActive(true);
            damageTakenNotCompleted.SetActive(false);
        }
        else
        {
            damageTakenCompleted.SetActive(false);
            damageTakenNotCompleted.SetActive(true);
        }
    }

    public void GoToNextLevel()
    {
        if (SceneManager.sceneCount > 1)
        {
            for (int i = 1; i < SceneManager.sceneCount; i++)
            {
                SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(i));
            }
        }
        GameManager.Instance.ResetPlayersPosition();
        LevelManager.Instance.LoadLevel(LevelManager.Instance.Level2);
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
}
