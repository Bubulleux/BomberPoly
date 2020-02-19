using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PowerUPPanelle : MonoBehaviour
{
    public Text Text;
    public int PowerUP;
    void Update()
    {
        try
        {
            Text.text = ClientManager.client.allPlayer[PhotonNetwork.LocalPlayer.ActorNumber].powerUps[PowerUP].ToString();
        }
        catch
        {
            Text.text = "0";
        }
    }
}
