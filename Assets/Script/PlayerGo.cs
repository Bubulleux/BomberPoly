﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class PlayerGo : MonoBehaviour
{
    public GameObject cam;
    private PhotonView Pv;
    public GameObject inBlock;
    public int viewIDclient;
    public float force;
    public GameObject gfx;

    public static readonly List<string> hats = new List<string>{ null, "CowboyHat", "Crown", "MagicianHat", "Mustache", "PoliceCap", "Sombrero", "VikingHelmet" };
    void Start()
    {
        Pv = GetComponent<PhotonView>();
        viewIDclient = ClientManager.client.GetComponent<PhotonView>().ViewID;
        
    }
    
    // Update is called once per frame
    void Update()
    {
        if (cam.activeSelf && !Pv.IsMine)
        {
            cam.SetActive(false);
        }
        string _hat = hats[ClientManager.client.allPlayer[Pv.Owner.ActorNumber].hat];
        if (_hat != null)
        {
            gfx.transform.Find("Hat").Find(_hat).gameObject.SetActive(true);
        }
        gfx.GetComponent<Renderer>().material.color = ClientManager.client.allPlayer[Pv.Owner.ActorNumber].color;
        if (Pv.IsMine)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
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
            if (GetComponent<Rigidbody>().velocity != Vector3.zero)
            {
                Quaternion _angVel = Quaternion.LookRotation(GetComponent<Rigidbody>().velocity);
                Quaternion _ang = Quaternion.RotateTowards(gfx.transform.rotation, _angVel, Time.fixedDeltaTime * 700f);
                gfx.transform.rotation = _ang;
            }
            
        }
    }
}
