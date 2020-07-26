using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine.UI;

public class RoomCreator : MonoBehaviour
{
    public Text nameRoom, maxPly;
    public void CreatRoom()
    {
        try
        {
            RoomOptions _room = new RoomOptions
            {
                MaxPlayers = byte.Parse(maxPly.text),
                CleanupCacheOnLeave = true
                
            };
            PhotonNetwork.CreateRoom(nameRoom.text, _room);

        }
        catch (Exception e)
        {
            transform.parent.GetComponent<ErrorBox>().Error(e.Message);
        }

    }
}
