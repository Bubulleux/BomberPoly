using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MysteryPower : MonoBehaviour
{
    private PhotonView photonView;
    private RoomManger roomManager;
    private void Start()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            enabled = false;
        }
        photonView = GetComponent<PhotonView>();
        roomManager = GameObject.Find("GameManager").GetComponent<RoomManger>();
    }
    public enum MysteryPowers
    {
        none,
        tp,
        megaBombe
    }

    public bool UseMyteryPower(Client _ply)
    {
        Debug.LogFormat("Player {0} use {1}", _ply.name, _ply.GetPly().mysteryPower.ToString());
        switch (_ply.GetPly().mysteryPower)
        {
            case MysteryPowers.tp:
                List<Vector2> _tpPosibilitis = new List<Vector2>();
                foreach (KeyValuePair<Vector2, BlockClass> _block in GameObject.Find("GameManager").GetComponent<RoomManger>().blocks)
                {
                    if (_block.Value.state == BlockState.destroyer)
                    {
                        _tpPosibilitis.Add(_block.Key);
                    }
                }
                Vector2 _pos = _tpPosibilitis[Random.Range(0, _tpPosibilitis.Count)];
                photonView.RPC("TP", RpcTarget.All, _pos.x + 0.5f, _pos.y + 0.5f);
                return true;
            case MysteryPowers.megaBombe:
                Vector3 _bombePos = new Vector3(Mathf.Floor(transform.position.x), 0f, Mathf.Floor(transform.position.z));
                foreach (GameObject _bombe in GameObject.FindGameObjectsWithTag("Bombe"))
                {
                    if (BM.Vec3To2int(_bombe.transform.position) == BM.Vec3To2int(_bombePos))
                    {
                        return false;
                    }
                }
                StartCoroutine(roomManager.TimerBombe(_bombePos, photonView.OwnerActorNr, true));
                return true;
        }
        return false;
    }
}
