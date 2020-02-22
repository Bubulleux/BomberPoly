using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class RoomSetting : MonoBehaviour
{
    public RoomManger roomManager;
    public InputField[] intsParm;
    void OnEnable()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            gameObject.SetActive(false);
        }
        int _i = 0;
        foreach(int _v in roomManager.roominfo.intsParm)
        {
            intsParm[_i].text = _v.ToString();
            _i += 1;
        }
    }

    public void Save()
    {
        int _i = 0;
        foreach (InputField _v in intsParm)
        {
            roomManager.roominfo.intsParm[_i] = int.Parse(_v.text);
            _i += 1;
        }
        gameObject.SetActive(false);
    }
}
