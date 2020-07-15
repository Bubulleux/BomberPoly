using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerMotorMasterClient : MonoBehaviour
{
    private PhotonView photonView;
    private RoomManger roomManager;
    private MysteryPower misteryPower;
    void Start()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            enabled = false;
        }
        photonView = GetComponent<PhotonView>();
        roomManager = GameObject.Find("GameManager").GetComponent<RoomManger>();
        misteryPower = GetComponent<MysteryPower>();
    }

    [PunRPC]
    void UsePower()
    {
        if (RoomManger.allPlayer[photonView.OwnerActorNr].GetPly().mysteryPower != MysteryPower.MysteryPowers.none)
        {
            if (misteryPower.UseMyteryPower(ClientManager.client.allPlayer[photonView.OwnerActorNr]))
            {
                RoomManger.allPlayer[photonView.OwnerActorNr].GetPly().mysteryPower = MysteryPower.MysteryPowers.none;
                roomManager.StreamSendData(StreamDataType.Players);
            }
        }
    }
}
