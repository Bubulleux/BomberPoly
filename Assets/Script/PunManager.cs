using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PunManager : MonoBehaviourPunCallbacks, ILobbyCallbacks
{

    public int multiplayerSceneIndex;
    public bool onLooby = false;
    public ClientState InfoCo;
    public List<RoomInfo> rooms;
    void Start()
    {
        Debug.ClearDeveloperConsole();
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.LogLevel = PunLogLevel.ErrorsOnly;
    }
    public void Update()
    {
        InfoCo = PhotonNetwork.NetworkClientState;
        onLooby = InfoCo == ClientState.JoinedLobby;
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log(rooms.Count);
            
        }
       
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connect To Master");
        TypedLobby lobbyData = new TypedLobby("Proto01Lobby", LobbyType.SqlLobby);
        PhotonNetwork.GetCustomRoomList(lobbyData, null);
        PhotonNetwork.JoinLobby();
        Debug.LogFormat("Client Connect in {0} ", PhotonNetwork.CloudRegion);
    }
    public override void OnJoinedLobby()
    {
        Debug.Log("Connect To lobby");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        //base.OnRoomListUpdate(roomList);
        rooms = roomList;
        Debug.Log("Room Find: " + roomList.Count);
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        PhotonNetwork.ConnectUsingSettings();
        Debug.LogWarning("Error to connect server " + cause);
    }
    private void OnFailedToConnect()
    {
        PhotonNetwork.ConnectUsingSettings();
        Debug.LogError("Error to Conectec Lobby");
    }

    public void ConnectToRoom()
    {
        if (onLooby)
        {
            RoomOptions _option = new RoomOptions { MaxPlayers = 5 };
            PhotonNetwork.JoinOrCreateRoom("main", _option, TypedLobby.Default);
        }
    }
    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.InRoom)
        {
            Debug.Log("Starting Game");
            PhotonNetwork.LoadLevel(multiplayerSceneIndex); 
        }
    }
    public override void OnEnable() => PhotonNetwork.AddCallbackTarget(this);
    public override void OnDisable() => PhotonNetwork.RemoveCallbackTarget(this);

}
