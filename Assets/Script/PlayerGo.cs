using System.Collections;
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
    public GameObject RobotGfx;
    public Vector3 deltaPos = new Vector3();

    public static readonly List<string> hats = new List<string>{ null, "CowboyHat", "Crown", "MagicianHat", "Mustache", "PoliceCap", "Sombrero", "VikingHelmet" };
    void Start()
    {
        Pv = GetComponent<PhotonView>();
        viewIDclient = ClientManager.client.GetComponent<PhotonView>().ViewID;

        deltaPos = transform.position;
    }
    void Update()
    {
        //if (cam.activeSelf && !Pv.IsMine)
        //{
        //    cam.SetActive(false);
        //}
        string _hat = hats[ClientManager.client.allPlayer[Pv.Owner.ActorNumber].var.hat];
        if (_hat != null)
        {
            gfx.transform.Find("Hat").Find(_hat).gameObject.SetActive(true);
        }
        RobotGfx.GetComponent<Renderer>().materials[2].color = ClientManager.client.allPlayer[Pv.Owner.ActorNumber].var.color;
        if (Pv.IsMine)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                ClientManager.client.Pv.RPC("CreatBombe", RpcTarget.MasterClient, transform.position.x, transform.position.z, PhotonNetwork.LocalPlayer.ActorNumber);
            }
        }
        foreach (KeyValuePair<int, PlayerData> _v in ClientManager.client.allPlayer)
        {
            if (_v.Value.var.palyerGOId != -1 && _v.Value.var.alive)
            {
                Collider _ply = PhotonView.Find(_v.Value.var.palyerGOId).gameObject.GetComponent<Collider>();
                Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), _ply);
            }
        }
    }
    public void FixedUpdate()
    {
        if (Pv.IsMine)
        {
            force = 30f + ((int)ClientManager.client.allPlayer[PhotonNetwork.LocalPlayer.ActorNumber].var.powerUps[PowerUps.speed] * 3f);
            Vector3 _goTo = new Vector3(Input.GetAxis("Horizontal") * force, 0f, Input.GetAxis("Vertical") * force);
            GetComponent<Rigidbody>().AddForce(_goTo, ForceMode.Force);
            Vector3 _newVel = GetComponent<Rigidbody>().velocity;
            
        }

        Vector3 _vel = (transform.position - deltaPos) / Time.deltaTime;
        if (_vel.magnitude > 0.1f )
        {
            Quaternion _angVel = Quaternion.LookRotation(_vel);
            Quaternion _ang = Quaternion.RotateTowards(gfx.transform.rotation, _angVel, Time.fixedDeltaTime * 700f);
            gfx.transform.rotation = _ang;
        }
        deltaPos = transform.position;
    }
}
