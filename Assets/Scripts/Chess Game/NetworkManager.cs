using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    private const string LEVEL = "level";
    private const byte MAX_PLAYERS = 2;
    [SerializeField] private ChessUIManager uiManager;
    [SerializeField] private GameInitializer gameInitializer;
    private MultiplayerChessGameController chessGameController;

    private ChessLevel playerLevel;

    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public void SetDependencies(MultiplayerChessGameController chessGameController)
    {
        this.chessGameController = chessGameController;
    }

    public void Connect()
    {
        if (PhotonNetwork.IsConnected)
        {
            Debug.LogError("Already connected, joining random room with level " + playerLevel);
            //PhotonNetwork.JoinRandomRoom(new ExitGames.Client.Photon.Hashtable() { { LEVEL, playerLevel } }, MAX_PLAYERS);
        }
        else
        {
            Debug.LogError("Connecting to server");
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    private void Update()
    {
        uiManager.SetConnectionStatusText(PhotonNetwork.NetworkClientState.ToString());
    }

    #region Photon Callbacks

    public override void OnConnectedToMaster()
    {
        Debug.LogError("Connected to server. Looking for random room with level " + playerLevel);
        PhotonNetwork.JoinRandomRoom(new ExitGames.Client.Photon.Hashtable() { { LEVEL, playerLevel } }, MAX_PLAYERS);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogError("Joining random room failed because of " + message + ". Creating new one with player level " + playerLevel);
        PhotonNetwork.CreateRoom(null, new RoomOptions
        {
            CustomRoomPropertiesForLobby = new string[] { LEVEL },
            MaxPlayers = MAX_PLAYERS,
            CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { LEVEL, playerLevel } },
        });
    }

    public override void OnJoinedRoom()
    {
        Debug.LogError("Player " + PhotonNetwork.LocalPlayer.ActorNumber + " joined a room with level: " + (ChessLevel)PhotonNetwork.CurrentRoom.CustomProperties[LEVEL]);
        gameInitializer.CreateMultiplayerBoard();
        PrepareTeamSelectionOptions();
        uiManager.ShowTeamSelectionScreen();
    }

    private void PrepareTeamSelectionOptions()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
        {
            foreach (var player in PhotonNetwork.CurrentRoom.Players)
            {
                if (player.Value != PhotonNetwork.LocalPlayer && player.Value.CustomProperties.ContainsKey("team"))
                {
                    var occupiedTeam = player.Value.CustomProperties["team"];
                    uiManager.RestrictTeamChoice((TeamColor)occupiedTeam);
                }
            }
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.LogError("Player " + newPlayer.ActorNumber + " entered a room");
    }

    public override void OnLeftRoom()
    {
        Debug.LogError("Left the room");
        PhotonNetwork.LocalPlayer.SetCustomProperties(null);
        uiManager.OnGameLaunched();
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.LogError("Player " + otherPlayer.ActorNumber + " left the room");
        if (chessGameController != null && chessGameController.IsGameInProgress())
        {
            TeamColor winner = (TeamColor)PhotonNetwork.LocalPlayer.CustomProperties["team"];
            uiManager.OnGameFinished(winner.ToString());
        }
    }

    #endregion

    public void SetPlayerLevel(ChessLevel level)
    {
        playerLevel = level;
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { LEVEL, level } });
    }

    public void SelectTeam(int team)
    {
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "team", team } });
        gameInitializer.InitializeMultiplayerController();
        chessGameController.SetLocalPlayer((TeamColor)team);
        chessGameController.StartNewGame();
        chessGameController.SetupCamera((TeamColor)team);
    }

    public bool IsRoomFull()
    {
        return PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers;
    }

}