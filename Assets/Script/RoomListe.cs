using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
public class RoomListe : MonoBehaviour
{
    public GameObject panelRoom;
    public Transform contente;
    public PunManager pun;
    void Start()
    {
        Initiaze();
    }

    void Update()
    {
        
    }

    void Initiaze()
    {
        foreach(GameObject _go in GameObject.FindGameObjectsWithTag("RoomPanel"))
        {
            Destroy(_go);
        }

        foreach(RoomInfo _room in pun.rooms)
        {
            GameObject _go = Instantiate(panelRoom, contente);
            _go.transform.Find("Name").GetComponent<Text>().text = _room.Name;
            _go.transform.Find("Player").GetComponent<Text>().text = _room.PlayerCount + "/" + _room.MaxPlayers;

        }
    }
}
