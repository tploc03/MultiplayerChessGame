using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameEndScreen : MonoBehaviour
{
    [SerializeField] private GameObject gameEndScreenUI;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button continueButton;

    private void Awake()
    {
        gameEndScreenUI.SetActive(false);
    }

    public void ShowSingleplayerEndScreen()
    {
        gameEndScreenUI.SetActive(true);
        resumeButton.gameObject.SetActive(true);
        continueButton.gameObject.SetActive(false);
        mainMenuButton.gameObject.SetActive(true);
    }

    public void ShowMultiplayerEndScreen()
    {
        gameEndScreenUI.SetActive(true);
        resumeButton.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(true);
        mainMenuButton.gameObject.SetActive(true);
    }

    public void ResumeGame()
    {
        gameEndScreenUI.SetActive(false);
    }

    public void ContinueGame()
    {
        gameEndScreenUI.SetActive(false);
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
