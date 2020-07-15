using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class PlayerControlerCl : MonoBehaviour
{
    public GameObject cam;
    private PhotonView Pv;
    public float force;

    void Start()
    {
        Pv = GetComponent<PhotonView>();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            ClientManager.client.Pv.RPC("CreatBombe", RpcTarget.MasterClient, transform.position.x, transform.position.z, PhotonNetwork.LocalPlayer.ActorNumber);
        }
        if (Input.GetMouseButton(1) || Input.GetKeyDown(KeyCode.LeftControl))
        {
            Pv.RPC("UsePower", RpcTarget.MasterClient);
        }
    }
    public void FixedUpdate()
    {
        force = 30f + ((int)ClientManager.client.allPlayer[PhotonNetwork.LocalPlayer.ActorNumber].GetPly().powerUps[PowerUps.speed] * 3f);
        Vector3 _goTo = new Vector3(Input.GetAxis("Horizontal") * force, 0f, Input.GetAxis("Vertical") * force);
        GetComponent<Rigidbody>().AddForce(_goTo, ForceMode.Force);
        Vector3 _newVel = GetComponent<Rigidbody>().velocity;
    }

    [PunRPC]
    void TP(float x , float y)
    {
        transform.position = new Vector3(x, 0.5f, y);
    }
}
