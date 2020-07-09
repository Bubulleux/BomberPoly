using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
//using Photon.Realtime;

public class PlayerCl : MonoBehaviour
{
    private Vector3 deltaPos = new Vector3();
    public GameObject gfx;
    public GameObject robotGFX;
    public static readonly List<string> hats = new List<string> { null, "CowboyHat", "Crown", "MagicianHat", "Mustache", "PoliceCap", "Sombrero", "VikingHelmet" };
    private bool Initialize = false;
    void Start()
    {
        
        foreach (GameObject _ply in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (_ply != gameObject)
            {
                Collider _plyColid = _ply.GetComponent<Collider>();
                Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), _plyColid);
            }
        }
        deltaPos = transform.position;
    }
    private void Update()
    {
        if (!Initialize && ClientManager.client.roomInfo.roundInfo != roundInfo.play)
        {
            
            PhotonView photonView = GetComponent<PhotonView>();
            Debug.Log(photonView.OwnerActorNr);
            PlayerData _ply = ClientManager.client.allPlayer[photonView.OwnerActorNr];
            string _hat = hats[_ply.var.hat];
            if (_hat != null)
            {
                gfx.transform.Find("Hat").Find(_hat).gameObject.SetActive(true);
            }
            robotGFX.GetComponent<Renderer>().materials[2].color = _ply.var.color;
            Initialize = true;

        }
    }
    private void FixedUpdate()
    {
        Vector3 _vel = (transform.position - deltaPos) / Time.deltaTime;
        if (_vel.magnitude > 0.1f)
        {
            Quaternion _angVel = Quaternion.LookRotation(_vel);
            Quaternion _ang = Quaternion.RotateTowards(gfx.transform.rotation, _angVel, Time.fixedDeltaTime * 700f);
            gfx.transform.rotation = _ang;
        }
        deltaPos = transform.position;
    }
}
