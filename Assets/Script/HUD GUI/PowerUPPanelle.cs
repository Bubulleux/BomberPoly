using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PowerUPPanelle : MonoBehaviour
{
    public Text Text;
    public PowerUps PowerUP;
    void Update()
    {
        try
        {
            Text.text = ((int)ClientManager.client.allPlayer[PhotonNetwork.LocalPlayer.ActorNumber].GetPly().powerUps[PowerUP]).ToString();
        }
        catch
        {
            Text.text = "0";
        }
    }
}
