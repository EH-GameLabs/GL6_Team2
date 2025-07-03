using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DemoCompletedUI : BaseUI
{
    [SerializeField] private Image image;
    [SerializeField] private Sprite sprite1;
    [SerializeField] private Sprite sprite2;

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(LevelManager.Instance.mainScene, LoadSceneMode.Single);
    }

    private void OnEnable()
    {
        StartCoroutine(ShowDemoCompleted());
    }

    private void OnDisable()
    {
        StopCoroutine(ShowDemoCompleted());
        image.sprite = sprite1; // Reset to the first sprite when the UI is disabled
    }

    private IEnumerator ShowDemoCompleted()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(0.5f);
            if (image.sprite == sprite1)
            {
                image.sprite = sprite2;
            }
            else
            {
                image.sprite = sprite1;
            }
        }
    }
}
