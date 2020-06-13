using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class RoomSetting : MonoBehaviour
{
    public RoomManger roomManager;
    public InputField mapSize;
    public InputField powerDensity;
    public Dropdown gameMode;
    void OnEnable()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            gameObject.SetActive(false);
        }
        mapSize.text = roomManager.roominfo.mapSize.ToString();
        powerDensity.text = (roomManager.roominfo.powerDensity * 100f).ToString();
        gameMode.value = (int)roomManager.roominfo.gameMode;
        
        
    }

    public void Save()
    {
        roomManager.roominfo.mapSize = int.Parse(mapSize.text);
        roomManager.roominfo.powerDensity = (int.Parse(powerDensity.text)/100f);
        roomManager.roominfo.gameMode = (GameModes)gameMode.value;
        roomManager.ClearScene(roundInfo.none);
        gameObject.SetActive(false);
    }
}
