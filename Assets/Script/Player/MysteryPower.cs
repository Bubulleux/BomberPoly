using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MysteryPower : MonoBehaviour
{
    private PhotonView photonView;
    private Map map;
    private RoomManger roomManager;
    private void Start()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            enabled = false;
        }
        photonView = GetComponent<PhotonView>();
        roomManager = GameObject.Find("GameManager").GetComponent<RoomManger>();
        map = GameObject.Find("Map").GetComponent<Map>();
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
                if (roomManager.roominfo.safeTp)
                {
                    Vector2Int tpPose;
                    do
                    {
                        tpPose = new Vector2Int(Random.Range(0, map.Maps.GetLength(0)), Random.Range(0, map.Maps.GetLength(1)));
                    } while (!(tpPose.x % 2 == 1 && tpPose.y % 2 == 1));
                    roomManager.MakeHole(tpPose.x, tpPose.y);
                    photonView.RPC("TP", RpcTarget.All, tpPose.x + 0.5f, tpPose.y + 0.5f);

                }
                else
                {
                    List<Vector2> _tpPosibilitis = new List<Vector2>();
                    for (int y = 0; y < map.Maps.GetLength(1); y++)
                    {
                        for (int x = 0; x < map.Maps.GetLength(0); x++)
                        {

                            if (map.Maps[x, y].state == BlockState.destroyer)
                            {
                                _tpPosibilitis.Add(new Vector2(x, y));
                            }
                        }
                    }
                    Vector2 _pos = _tpPosibilitis[Random.Range(0, _tpPosibilitis.Count)];
                    photonView.RPC("TP", RpcTarget.All, _pos.x + 0.5f, _pos.y + 0.5f);
                }
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
