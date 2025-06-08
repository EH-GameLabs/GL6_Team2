using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionUI : BaseUI
{
    public void GoToMainMenu()
    {
        SceneManager.LoadScene(0);
        Time.timeScale = 1f;
    }
}
