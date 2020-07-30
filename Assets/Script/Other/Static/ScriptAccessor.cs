using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public static class ScriptAccessor 
{
    public static RoomManger GetRoomManager()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            return GameObject.Find("GameManager").GetComponent<RoomManger>();
        }
        Debug.LogError("Your are not the masterClient");
        return null;
    }

    public static ClientManager GetClientManager()
    {
        return GameObject.Find("GameManager").GetComponent<ClientManager>();
    }
}
