using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Scoretable : MonoBehaviour
{
    public GameObject plyCellulePrefab;
    public GameObject ScoreBorad;
    public Transform plyTable;
    public bool enabelScoreBorad;
    public ClientManager client;
    public Client winer;
    public Text mainText;
    void Start()
    {
        
    }
    
    void Update()
    {
        if (enabelScoreBorad != ScoreBorad.activeSelf)
        {
            ScoreBorad.SetActive(enabelScoreBorad);
            if (enabelScoreBorad)
            {
                InitialazePlayerTable();
            }
        }
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            enabelScoreBorad = false;
        }
        if (Input.GetKeyDown(KeyCode.Tab) || (client.roomInfo.roundInfo != roundInfo.play && !enabelScoreBorad))
        {
            enabelScoreBorad = true;
            if (client.roomInfo.roundInfo != roundInfo.play)
            {
                foreach(KeyValuePair<int, Client> _ply in client.allPlayer)
                {
                    if (_ply.Value.alive)
                    {
                        winer = _ply.Value;
                    }
                }
            }
        }
        if (client.roomInfo.roundInfo == roundInfo.play && winer != null)
        {
            winer = null;
        }
        if (enabelScoreBorad && client.roomInfo.roundInfo == roundInfo.play && !Input.GetKey(KeyCode.Tab))
        {
            enabelScoreBorad = false;
        }
    }

    public void InitialazePlayerTable()
    {
        for (int i = 0; i < plyTable.childCount; i++)
        {
            Destroy(plyTable.GetChild(i).gameObject);
        }
        foreach(KeyValuePair<int, Client> _ply in client.allPlayer)
        {
            Transform _plyCellule = Instantiate(plyCellulePrefab, plyTable).transform;
            _plyCellule.Find("Alive").GetComponent<Toggle>().isOn = _ply.Value.alive;
            _plyCellule.Find("Name").GetComponent<Text>().text = _ply.Value.name;
            _plyCellule.Find("Name").GetComponent<Text>().color = _ply.Value.color * new Color(0.75f, 0.75f, 0.75f);
            _plyCellule.Find("Win").GetComponent<Text>().text = _ply.Value.win.ToString();
            _plyCellule.Find("Kill").GetComponent<Text>().text = _ply.Value.kill.ToString();
            _plyCellule.Find("Ping").GetComponent<Text>().text = _ply.Value.ping.ToString();
        }
        if (winer == null)
        {
            mainText.text = PhotonNetwork.CurrentRoom.Name;
        }
        else
        {
            mainText.text = winer.name + " Won!";
        }
    }
}
