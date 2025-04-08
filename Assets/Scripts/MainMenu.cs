using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("Main");
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    private void OnDestroy()
    {
        DisconnectFromPhoton();
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