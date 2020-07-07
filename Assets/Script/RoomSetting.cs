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
    public InputField timeBeforeShrinking;
    public InputField timeBetweenShrinking;
    void OnEnable()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            gameObject.SetActive(false);
        }
        mapSize.text = roomManager.roominfo.mapSize.ToString();
        powerDensity.text = (roomManager.roominfo.powerDensity * 100f).ToString();
        timeBeforeShrinking.text = roomManager.roominfo.timeBeforeShrinking.ToString();
        timeBetweenShrinking.text = roomManager.roominfo.timeBetweenShrinking.ToString();
        
        
    }

    public void Save()
    {
        roomManager.roominfo.mapSize = int.Parse(mapSize.text);
        roomManager.roominfo.powerDensity = (int.Parse(powerDensity.text)/100f);
        roomManager.roominfo.timeBeforeShrinking = int.Parse(timeBeforeShrinking.text);
        roomManager.ClearScene(roundInfo.none);
        gameObject.SetActive(false);
    }
}
