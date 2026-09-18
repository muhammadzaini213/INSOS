using Slafurry.System.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private string gameSceneName = "02_ChooseGenderMenu";

    [SerializeField]
    private string aboutMenuSceneName = "AboutMenu";

    [SerializeField]
    private string settingsMenuSceneName = "SettingsMenu";

    [SerializeField]
    private string activityMenuSceneName = "03_ChooseActivityMenu";

    public void StartGame()
    {
        if (!string.IsNullOrEmpty(PlayerData.PlayerName) && PlayerPrefs.HasKey("PlayerGender"))
        {
            SceneManager.LoadScene(activityMenuSceneName);
        }
        else
        {
            SceneManager.LoadScene(gameSceneName);
        }
    }

    public void ContinueGame()
    {
        SceneManager.LoadScene(activityMenuSceneName);
    }

    public void About()
    {
        SceneManager.LoadScene(aboutMenuSceneName);
    }

    public void Settings()
    {
        SceneManager.LoadScene(settingsMenuSceneName);
    }

    public void Exit()
    {
        Application.Quit();
    }
}
