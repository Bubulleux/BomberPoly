using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class RoomSetting : MonoBehaviour
{
    public RoomManger roomManager;
    public InputField mapSize;
    public InputField boxDensity;
    public InputField timeBeforeShrinking;
    public InputField timeBetweenShrinking;
    public InputField powerDensity;
    public InputField mysteryPowerDensity;
    public Toggle safeTP;
    void OnEnable()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            gameObject.SetActive(false);
        }
        mapSize.text = roomManager.roominfo.mapSize.ToString();
        boxDensity.text = (roomManager.roominfo.boxDensity * 100f).ToString();
        powerDensity.text = (roomManager.roominfo.powerDensity * 100f).ToString();
        mysteryPowerDensity.text = (roomManager.roominfo.mysteryPowerDensity * 100f).ToString();
        timeBeforeShrinking.text = roomManager.roominfo.timeBeforeShrinking.ToString();
        timeBetweenShrinking.text = roomManager.roominfo.timeBetweenShrinking.ToString();
        safeTP.isOn = roomManager.roominfo.safeTp;
        
        
    }

    public void Save()
    {
        roomManager.roominfo.mapSize = int.Parse(mapSize.text);
        roomManager.roominfo.boxDensity = (int.Parse(boxDensity.text)/100f);
        roomManager.roominfo.powerDensity = (int.Parse(powerDensity.text)/100f);
        roomManager.roominfo.mysteryPowerDensity = (int.Parse(mysteryPowerDensity.text)/100f);
        roomManager.roominfo.timeBeforeShrinking = int.Parse(timeBeforeShrinking.text);
        roomManager.roominfo.timeBetweenShrinking = int.Parse(timeBetweenShrinking.text);
        roomManager.roominfo.safeTp = safeTP.isOn;
        roomManager.ClearScene(roundInfo.none);
        gameObject.SetActive(false);
    }
}
