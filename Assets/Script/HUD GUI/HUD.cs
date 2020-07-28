using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class HUD : MonoBehaviour
{
    public ClientManager client;
    public Text plyAlive;
    public Text timer;
    public Text mysteriPower; 
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (client.allPlayer.Count == 0)
        {
            return;
        }
        if (client.roomInfo.roundInfo == roundInfo.play)
        {

            int _sec = Mathf.FloorToInt(client.roomInfo.cooldown);
            timer.text = string.Format("{0}{1} : {2}{3}", Mathf.Floor(_sec / 60) < 10 ? "0" : "", Mathf.Floor(_sec / 60), _sec % 60 < 10 ? "0" : "" , _sec % 60);
            if (client.roomInfo.shrinking != 0)
            {
                timer.color = _sec % 2 == 0 ? Color.red : Color.black;
            }
        }
        else
        {
            timer.text = "00 : 00";
            timer.color = Color.black;
        }
        int _plyAlive = 0;
        foreach (KeyValuePair<int, Client> _ply in client.allPlayer)
        {
            if (_ply.Value.alive)
            {
                _plyAlive++;
            }
        }
        plyAlive.text = "Alive : " + _plyAlive;
        if (client.LocalPly().GetPly() != null)
        {
            mysteriPower.text = client.LocalPly().GetPly().mysteryPower.ToString();
        }
        else
        {
            mysteriPower.text = "0";
        }
    }
}
