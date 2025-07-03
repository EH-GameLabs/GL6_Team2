using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectorUI : BaseUI
{
    public void StartLevel(int levelIndex)
    {
        string levelString = "";

        switch (levelIndex)
        {
            case 1:
                levelString = LevelManager.Instance.Level1;
                break;
            case 2:
                levelString = LevelManager.Instance.Level2;
                break;
            default:
                Debug.LogError("Invalid level index: " + levelIndex);
                return;
        }

        // se ho già scene caricate, le rimuovo
        if (SceneManager.sceneCount > 1)
        {
            for (int i = 1; i < SceneManager.sceneCount; i++)
            {
                SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(i));
            }
        }
        LevelManager.Instance.LoadLevel(levelString);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(LevelManager.Instance.mainScene, LoadSceneMode.Single);
    }
}
