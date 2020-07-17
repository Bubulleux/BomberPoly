using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Newtonsoft.Json;
//using Photon.Realtime;

public class PlayerCl : MonoBehaviour, IPunObservable
{
    public GameObject gfx;
    public GameObject robotGFX;

    public MysteryPower.MysteryPowers mysteryPower;
    public Dictionary<PowerUps, int> powerUps = new Dictionary<PowerUps, int>();
    public int BombeCount;

    private Vector3 deltaPos = new Vector3();
    private bool Initialize = false;

    public static readonly List<string> hats = new List<string> { null, "CowboyHat", "Crown", "MagicianHat", "Mustache", "PoliceCap", "Sombrero", "VikingHelmet" };
    void Start()
    {
        powerUps = new Dictionary<PowerUps, int>();
        powerUps.Add(PowerUps.moreBombe, 0);
        powerUps.Add(PowerUps.moreRiadusse, 0);
        powerUps.Add(PowerUps.speed, 0);
        BombeCount = 0;
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
        if (!Initialize && ClientManager.client.roomInfo.roundInfo == roundInfo.play)
        {

            PhotonView photonView = GetComponent<PhotonView>();
            Client _ply = ClientManager.client.allPlayer[photonView.OwnerActorNr];
            string _hat = hats[_ply.hat];
            if (_hat != null)
            {
                gfx.transform.Find("Hat").Find(_hat).gameObject.SetActive(true);
            }
            robotGFX.GetComponent<Renderer>().materials[2].color = _ply.color;
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
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (PhotonNetwork.IsMasterClient && stream.IsWriting)
        {
            stream.SendNext(mysteryPower);
            stream.SendNext(BombeCount);
            stream.SendNext(JsonConvert.SerializeObject(powerUps));
        }
        if (GetComponent<PhotonView>().IsMine && stream.IsReading)
        {
            mysteryPower = (MysteryPower.MysteryPowers)stream.ReceiveNext();
            BombeCount = (int)stream.ReceiveNext();
            powerUps = JsonConvert.DeserializeObject<Dictionary<PowerUps, int>>((string)stream.ReceiveNext());
        }
    }
}
