using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RoomManger : MonoBehaviourPunCallbacks
{
    public PhotonView Pv;
    public GameObject Block;
    public GameObject bombe;
    public GameObject explosion;
    public ClientManager client;

    public int sizeTerrain = 20;
    public bool debugRound;

    public roundInfo roundStat = roundInfo.none;
    
    public GameObject PowerUp;
    
    public static Dictionary<int,PlayerData> allPlayer = new Dictionary<int, PlayerData>();
    public Dictionary<Vector2, BlockClass> Blocks = new Dictionary<Vector2, BlockClass>();

    public StreamManager stream;
    public static RoomManger RoomManagerCom;

    public GameObject plyGO;


    void Start()
    {
        RoomManagerCom = GetComponent<RoomManger>();
        if (!PhotonNetwork.IsMasterClient)
        {
            GetComponent<RoomManger>().enabled = false;
            return;
        }
        Pv = GetComponent<PhotonView>();
        client = GetComponent<ClientManager>();
        stream = PhotonNetwork.Instantiate("Stream", Vector3.zero, Quaternion.identity).GetComponent<StreamManager>();
        
    }
    
    void Update()
    {
        if ((roundStat == roundInfo.none && client.playerCount >= 2) || (debugRound && Input.GetKeyDown(KeyCode.F2)))
        {
            StartRound();
        }
        
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.F1))
        {
            debugRound = !debugRound;
            Debug.LogWarning("Debug Mod " + debugRound);
        }

        if (GameObject.FindGameObjectsWithTag("Player").Length <= 1 && !debugRound && roundStat == roundInfo.play)
        {
            GameObject _winer = GameObject.FindGameObjectWithTag("Player");
            allPlayer[_winer.GetPhotonView().OwnerActorNr].win += 1;
            StartCoroutine(EndRound());

        }

        stream.allPlayer = allPlayer;
    }

    public void StartRound()
    {
        roundStat = roundInfo.load;
        sizeTerrain = (sizeTerrain % 2 == 1) ? sizeTerrain + 1 : sizeTerrain;
        Pv.RPC("DestroyBlock", RpcTarget.AllBuffered);
        StartCoroutine(LoadRound());
        
    }

    IEnumerator EndRound()
    {
        yield return new WaitForSeconds(3f);
        roundStat = roundInfo.end;
        GameObject _winer = GameObject.FindGameObjectWithTag("Player");
        Pv.RPC("RoundEnd", RpcTarget.All, _winer.GetPhotonView().Owner.ActorNumber);
        yield return new WaitForSeconds(5f);
        roundStat = roundInfo.none;
    }
    
    IEnumerator LoadRound()
    {
        Blocks.Clear();
        Blocks = new Dictionary<Vector2, BlockClass>();
        for (int y = 0; y <= sizeTerrain; y++)
        {
            for (int x = 0; x <= sizeTerrain; x++)
            {
                bool _unbreak = ((x % 2 == 0) && (y % 2 == 0)) || (x == 0) || (x == sizeTerrain) || (y == 0) || (y == sizeTerrain);
                //client.MakeBlock(_unbreak, Random.Range(0, 5) != 0, );
                Blocks.Add(new Vector2Int(x, y), new BlockClass());
                Blocks[new Vector2Int(x, y)].state = _unbreak ? BlockState.unbrekable : Random.Range(0, 5) == 0 ? BlockState.destroyer : BlockState.brekable;
                if (Blocks[new Vector2Int(x, y)].state != BlockState.destroyer && Random.Range(0, 4) == 0)
                {
                    Blocks[new Vector2Int(x, y)].PowerUp = Random.Range(1, 4);
                }
                //yield return new WaitForFixedUpdate();
            }
        }
        yield return new WaitForSeconds(2f);

        foreach(GameObject _go in GameObject.FindGameObjectsWithTag("Bombe"))
        {
            PhotonNetwork.Destroy(_go);
        }
        foreach(GameObject _go in GameObject.FindGameObjectsWithTag("Player"))
        {
            //_go.GetPhotonView().RequestOwnership();
            //PhotonNetwork.Destroy(_go);
            StartCoroutine(KillPlayer(_go.GetPhotonView().ViewID));
        }


        foreach(KeyValuePair<int, PlayerData> _ply in allPlayer)
        {
            if (IsFind(_ply.Key))
            {
                allPlayer[_ply.Key].ClassToOrigine();
                Vector2Int _spawnPos = new Vector2Int(Random.Range(1, 8) * 2 - 1, Random.Range(1, 8) * 2 - 1);
                Pv.RPC("SpawnHere", RpcTarget.All, _spawnPos.x, _spawnPos.y);
                GameObject _player = PhotonNetwork.Instantiate(plyGO.name, new Vector3(_spawnPos.x + 0.5f, 0.5f, _spawnPos.y + 0.5f), Quaternion.identity);
                _player.GetPhotonView().TransferOwnership(_ply.Key);
                allPlayer[_ply.Key].alive = true;
                allPlayer[_ply.Key].palyerGOId = _player.GetPhotonView().ViewID;
            }
            else
            {
                allPlayer.Remove(_ply.Key);
            }
        }
        yield return new WaitForSeconds(0.5f);
        roundStat = roundInfo.play;
        Pv.RPC("StartRound", RpcTarget.All);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        if (roundStat == roundInfo.play)
        {
            PhotonView _pv = PhotonView.Find(allPlayer[otherPlayer.ActorNumber].palyerGOId);
            _pv.TransferOwnership(PhotonNetwork.LocalPlayer);
            PhotonNetwork.Destroy(_pv.gameObject);

        }
        allPlayer.Remove(otherPlayer.ActorNumber);
    }

    public bool IsFind(int _ply)
    {
        foreach(Player _pl in PhotonNetwork.PlayerList)
        {
            if (_pl.ActorNumber == _ply)
            {
                return true;
            }
        }
        return false;
    }


    [PunRPC]
    void DestroyBlock(int _x, int _y)
    {
        Debug.Log(new Vector2(_x, _y) + "  " + Blocks.ContainsKey(new Vector2(_x, _y)) + "  " + Blocks.Count + "   " + PhotonNetwork.IsMasterClient);
        if (Blocks[new Vector2(_x, _y)].state == BlockState.brekable)
        {
            Blocks[new Vector2(_x, _y)].state = BlockState.destroyer;
        }
    }

    [PunRPC]
    void TakePowerUp(int _why, int _what)
    {
        allPlayer[_why].powerUps[_what - 1] += 1;
    }
    [PunRPC]
    void DestroyPower(int x, int y)
    {
        Blocks[new Vector2(x, y)].PowerUp = 0;
    }

    [PunRPC]
    void PlayerAlive(int _view, bool _alive)
    {
        allPlayer[_view].alive = _alive;
    }

    [PunRPC]
    void PlayerConnecte(string _name, int _viewId)
    {
        PlayerData _plyData = new PlayerData();
        _plyData.name = _name;
        _plyData.color = new Color(Random.value, Random.value, Random.value);
        Debug.LogFormat("<color=green> Player {0} has been conected id: {1} </color>", _name, _viewId);
        allPlayer.Add(_viewId, _plyData);
    }

    [PunRPC]
    void SpawnHere(int _x, int _y)
    {
        Vector2Int _spawnPos = new Vector2Int(_x, _y);
        Vector2Int[] _blockDestroy = { _spawnPos + Vector2Int.up, _spawnPos + Vector2Int.down, _spawnPos + Vector2Int.left, _spawnPos + Vector2Int.right, _spawnPos };
        foreach (Vector2Int _posBlockDestroy in _blockDestroy)
        {
            if (Blocks[_posBlockDestroy].state == BlockState.brekable)
            {
                Blocks[_posBlockDestroy].state = BlockState.destroyer;
                Blocks[_posBlockDestroy].PowerUp = 0;
            }
            
        }
    }

    [PunRPC]
    void CreatBombe(float _x, float _y, int _owner)
    {
        Vector3 _bombePos = new Vector3(Mathf.Floor(_x), 0f, Mathf.Floor(_y));
        bool _bombeHere = false;
        foreach(GameObject _bombe in GameObject.FindGameObjectsWithTag("Bombe"))
        {
            if (BM.Vec3To2int(_bombe.transform.position) == BM.Vec3To2int(_bombePos))
            {
                _bombeHere = true;
                break;
            }
        }

        if (!_bombeHere && allPlayer[_owner].BombeCount < allPlayer[_owner].powerUps[2]+1)
        {
            GameObject _go = PhotonNetwork.Instantiate(bombe.name, _bombePos, Quaternion.identity);
            allPlayer[_owner].BombeCount += 1;
            if (!allPlayer[_owner].PowerUpsTrueOrFalse[0])
            {
                StartCoroutine(TimerBombe(_go, _owner));
            }
        }
    }
    IEnumerator TimerBombe(GameObject _go, int _owner)
    {
        yield return new WaitForSeconds(3f);
        Explositon(_go, _owner);
    }

    public void Explositon( GameObject _bombe, int _owner)
    {
        Vector3 _pos = _bombe.transform.position;
        PhotonNetwork.Instantiate(explosion.name, _pos + new Vector3(0.5f, 0.3f, 0.5f), Quaternion.identity);
        Vector2Int _bombPos = new Vector2Int(Mathf.FloorToInt(_pos.x), Mathf.FloorToInt(_pos.z));
        ExploseHere(_bombPos, _owner);
        Vector2Int[] _alldir = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        allPlayer[_owner].BombeCount -= 1;
        foreach (Vector2Int _dir in _alldir)
        {
            for (int i = 1; i <= (allPlayer[_owner].powerUps[0] + 1); i++)
            {
                Vector2Int _expPos = new Vector2Int(_bombPos.x, _bombPos.y) + (new Vector2Int(_dir.x, _dir.y) * i);
                PhotonNetwork.Instantiate(explosion.name, new Vector3(_expPos.x + 0.5f, 0.3f, _expPos.y + 0.5f), Quaternion.identity);
                ExploseHere(_expPos, _owner);
                if (Blocks[_expPos].state != BlockState.destroyer)
                {
                    Pv.RPC("DestroyBlock", RpcTarget.MasterClient, _expPos.x, _expPos.y);
                    break;
                }
            }
        }
        PhotonNetwork.Destroy(_bombe);
    }

    private void ExploseHere(Vector2 pose, int _owner)
    {
        foreach(KeyValuePair<int, PlayerData> _ply in allPlayer)
        {
            if (_ply.Value.alive)
            {
                Vector2Int _plypose = BM.Vec3To2int(PhotonView.Find(_ply.Value.palyerGOId).gameObject.transform.position);
                if (_plypose == pose)
                {
                    allPlayer[_ply.Key].alive = false;
                    //PhotonView.Find(_ply.Value.palyerGOId).RequestOwnership();
                    //PhotonNetwork.Destroy(PhotonView.Find(_ply.Value.palyerGOId).gameObject);
                    StartCoroutine(KillPlayer(_ply.Value.palyerGOId));
                    if (_ply.Key != _owner)
                    {
                        allPlayer[_owner].kill += 1;
                    }
                    else
                    {
                        allPlayer[_owner].kill -= 1;
                    }

                }
            }
                
        }
    }

    IEnumerator KillPlayer(int _ply)
    {
        PhotonView.Find(_ply).RequestOwnership();
        while(PhotonView.Find(_ply).Owner != PhotonNetwork.LocalPlayer)
        {
            yield return new WaitForSeconds(0.01f); 
        }
        PhotonNetwork.Destroy(PhotonView.Find(_ply).gameObject);
        yield return null;
    }

}

public class PlayerData
{
    public string name;
    public int kill;
    public int win;
    public bool alive = false;
    public int palyerGOId = -1;
    public Color color;
    public float[] powerUps = new float[3];
    public bool[] PowerUpsTrueOrFalse = new bool[1];
    public int BombeCount;
    public void ClassToOrigine()
    {
        powerUps = new float[3];
        BombeCount = 0;
        palyerGOId = -1;
        alive = false;
        PowerUpsTrueOrFalse = new bool[1];
    }
}
public class BlockClass
{
    public BlockState state;
    public int PowerUp = 0;

}
public enum roundInfo
{
    none, play, end, load
}
