using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class PlayerGo : MonoBehaviour
{
    public Camera cam;
    private PhotonView Pv;
    public GameObject inBlock;
    public int viewIDclient;
    public float force;
    void Start()
    {
        Pv = GetComponent<PhotonView>();
        viewIDclient = ClientManager.client.GetComponent<PhotonView>().ViewID;
        
    }
    
    // Update is called once per frame
    void Update()
    {
        if (cam.enabled && !Pv.IsMine)
        {
            cam.enabled = false;
        }
        GetComponent<Renderer>().material.color = ClientManager.client.allPlayer[Pv.Owner.ActorNumber].color;
        if (Pv.IsMine)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ClientManager.client.Pv.RPC("CreatBombe", RpcTarget.MasterClient, transform.position.x, transform.position.z, PhotonNetwork.LocalPlayer.ActorNumber);
            }
        }
    }
    public void FixedUpdate()
    {
        if (Pv.IsMine)
        {
            force = 30f + (ClientManager.client.allPlayer[PhotonNetwork.LocalPlayer.ActorNumber].powerUps[1] * 3f);
            Vector3 _goTo = new Vector3(Input.GetAxis("Horizontal") * force, 0f, Input.GetAxis("Vertical") * force);
            GetComponent<Rigidbody>().AddForce(_goTo, ForceMode.Force);
        }
    }
}
