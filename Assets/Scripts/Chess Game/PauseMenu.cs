using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System.Collections;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    [SerializeField] private GameObject pauseMenuUI;

    private void Awake()
    {
        pauseMenuUI.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1f;
        GameIsPaused = false;

        StartCoroutine(LeaveRoomAndReturnToMenu());
    }

    private IEnumerator LeaveRoomAndReturnToMenu()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            while (PhotonNetwork.InRoom)
                yield return null;
        }

        CleanupGameObjects();
        SceneManager.LoadScene("MainMenu");
    }

    private void CleanupGameObjects()
    {
        var controller = Object.FindFirstObjectByType<ChessGameController>();
        if (controller != null) Destroy(controller.gameObject);

        var board = Object.FindFirstObjectByType<Board>();
        if (board != null) Destroy(board.gameObject);

        var cameraSetup = Object.FindFirstObjectByType<CameraSetup>();
        if (cameraSetup != null) Destroy(cameraSetup.gameObject);

        var uiManager = Object.FindFirstObjectByType<ChessUIManager>();
        if (uiManager != null) Destroy(uiManager.gameObject);

        var initializer = Object.FindFirstObjectByType<GameInitializer>();
        if (initializer != null) Destroy(initializer.gameObject);

        PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.LocalPlayer);
    }


    private void DisconnectFromPhoton()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom(false);
        }
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
    }
}