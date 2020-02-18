﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PunManager : MonoBehaviourPunCallbacks
{

    public int multiplayerSceneIndex;
    public bool onLooby = false;
    public ClientState InfoCo;
    public List<RoomInfo> rooms;
    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
    }
    public void Update()
    {
        InfoCo = PhotonNetwork.NetworkClientState;
        onLooby = InfoCo == ClientState.ConnectedToMasterServer;
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
        rooms = roomList;
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
    //public override void OnEnable() => PhotonNetwork.AddCallbackTarget(this);
    //public override void OnDisable() => PhotonNetwork.RemoveCallbackTarget(this);

}
