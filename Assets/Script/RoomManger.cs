using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Newtonsoft.Json;

public class RoomManger : MonoBehaviourPunCallbacks
{
    public PhotonView Pv;
    public GameObject Block;
    public GameObject bombe;
    public GameObject explosion;
    public ClientManager client;
    public GameObject PowerUp;
    public StreamManager stream;
    public GameObject plyGO;

    public static Dictionary<int,PlayerData> allPlayer = new Dictionary<int, PlayerData>();
    public Dictionary<Vector2, BlockClass> blocks = new Dictionary<Vector2, BlockClass>();
    public RoomInfoClass roominfo = new RoomInfoClass();

    public static RoomManger RoomManagerCom;

    void Awake()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            GetComponent<RoomManger>().enabled = false;
            return;
        }
        ClearDataRoom();
        allPlayer.Clear();
        Pv = GetComponent<PhotonView>();
        client = GetComponent<ClientManager>();
        //stream = PhotonNetwork.Instantiate("Stream", Vector3.zero, Quaternion.identity).GetComponent<StreamManager>();
    }
    
    void Update()
    {
        if (roominfo.roundInfo == roundInfo.none && client.playerCount >= 2 && !roominfo.debugRound)
        {
            StartCoroutine(StartRound(false));
        }
        if (roominfo.debugRound && Input.GetKeyDown(KeyCode.F3))
        {
            StartCoroutine(StartRound(true));
        }
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.F1))
        {
            roominfo.debugRound = !roominfo.debugRound;
            Debug.LogWarning("Debug Mod " + roominfo.debugRound);
        }
        
        if ( roominfo.roundInfo == roundInfo.play)
        {
            List<int> _plyAlive = new List<int>();
            foreach (KeyValuePair<int, PlayerData> _v in allPlayer)
            {
                if (_v.Value.var.alive)
                {
                    _plyAlive.Add(_v.Key);
                }
            }
            if (_plyAlive.Count < 2)
            {
                int _winer = 0;
                if (_plyAlive.Count == 0)
                {
                    _winer = -1;
                }
                else
                {
                    _winer = _plyAlive[0];
                }
                roominfo.roundInfo = roundInfo.preEnd;
                StartCoroutine(EndRound(_winer));
            }
        }
        //stream.allPlayer = allPlayer;

        if (Input.GetKeyDown(KeyCode.F6))
        {
            Debug.LogFormat("Room State {0}, Count Player {1}",
                roominfo.roundInfo,
                allPlayer.Count);
        }
        if (Input.GetKeyDown(KeyCode.F5))
        {
            ClearDataRoom();
        }
    }

    public void ClearDataRoom()
    {
        RoomManagerCom = this;
        roominfo = new RoomInfoClass();
        blocks.Clear();
        ClearScene(roundInfo.none);
        Debug.Log("<color=red> Room's Data was clear </color>");
    }

    public IEnumerator EndRound(int _winer)
    {
        //stream.WhyUpdate(0);
        StreamSendData(StreamDataType.Players);
        yield return new WaitForSeconds(3f);
        Pv.RPC("RoundEnd", RpcTarget.All, _winer);
        ClearScene(roundInfo.end);
        yield return new WaitForSeconds(5f);
        roominfo.roundInfo = roundInfo.none;
    }

    public void ClearScene(roundInfo _r)
    {
        Pv.RPC("ClearScene", RpcTarget.All);
        roominfo.roundInfo = _r;
        foreach (GameObject _go in GameObject.FindGameObjectsWithTag("Bombe"))
        {
            PhotonNetwork.Destroy(_go);
        }
    }
    IEnumerator StartRound(bool _debug)
    {
        ClearScene(roundInfo.load);
        Debug.LogFormat("<color=blue> {0} Round Start </color>", _debug ? "Debug" :"");
        roominfo.mapSize = (roominfo.mapSize % 2 == 1) ? roominfo.mapSize + 1 : roominfo.mapSize;
        Pv.RPC("DestroyBlock", RpcTarget.AllBuffered);
        foreach (GameObject _go in GameObject.FindGameObjectsWithTag("Player"))
        {
            //_go.GetPhotonView().RequestOwnership();
            //PhotonNetwork.Destroy(_go);
            StartCoroutine(KillPlayer(_go.GetPhotonView().ViewID));
        }
        blocks.Clear();
        blocks = new Dictionary<Vector2, BlockClass>();
        Debug.Log(allPlayer.Count);
        if (allPlayer.Count < 2)
        {
            int _plyInt = -1;
            PlayerVar _var = new PlayerVar()
            {
                
                name = "Bot",
                color = Color.black,
                bot = true
            };
            PlayerData _plyData = new PlayerData(_var);
            try
            {
                allPlayer.Add(_plyInt, _plyData);
            }
            catch { }
        }
        for (int y = 0; y <= roominfo.mapSize; y++)
        {
            for (int x = 0; x <= roominfo.mapSize; x++)
            {
                bool _unbreak = ((x % 2 == 0) && (y % 2 == 0)) || (x == 0) || (x == roominfo.mapSize) || (y == 0) || (y == roominfo.mapSize);
                //client.MakeBlock(_unbreak, Random.Range(0, 5) != 0, );
                blocks.Add(new Vector2Int(x, y), new BlockClass());
                blocks[new Vector2Int(x, y)].state = _unbreak ? BlockState.unbrekable : Random.Range(0, 5) == 0 ? BlockState.destroyer : BlockState.brekable;
                if (blocks[new Vector2Int(x, y)].state != BlockState.destroyer && (Random.Range(0f,1f) < roominfo.powerDensity))
                {
                    blocks[new Vector2Int(x, y)].PowerUp = (PowerUps)Random.Range(1, 4);
                }
                //yield return new WaitForFixedUpdate();
            }
        }
        yield return new WaitForSeconds(2f);
        List<int> _remove = new List<int>();
        foreach (KeyValuePair<int, PlayerData> _ply in allPlayer)
        {
            //Debug.Log(_ply.Key);
            if (IsFind(_ply.Key) || (_ply.Value.var.bot && _debug && allPlayer.Count < 3))
            {
                allPlayer[_ply.Key].ClassToOrigine();
                Vector2Int _spawnPos = new Vector2Int(Random.Range(1, 8) * 2 - 1, Random.Range(1, 8) * 2 - 1);
                Pv.RPC("SpawnHere", RpcTarget.All, _spawnPos.x, _spawnPos.y);
                GameObject _player = PhotonNetwork.Instantiate(plyGO.name, new Vector3(_spawnPos.x + 0.5f, 0.5f, _spawnPos.y + 0.5f), Quaternion.identity);
                _player.GetPhotonView().TransferOwnership(_ply.Key);
                allPlayer[_ply.Key].var.alive = true;
                allPlayer[_ply.Key].var.palyerGOId = _player.GetPhotonView().ViewID;
                if (_ply.Value.var.bot)
                {
                    _player.GetComponent<PlayerGo>().enabled = false;
                    _player.GetComponent<PlayerGo>().cam.SetActive(false);
                }
            }
            else
            {
                _remove.Add(_ply.Key);
            }

            Debug.Log("Spawn Ply:" + _ply.Key + " " + IsFind(_ply.Key) + "  " + _ply.Value.var.bot+ "   "+ allPlayer[_ply.Key].var.palyerGOId);
        }
        foreach(int _ply in _remove)
        {
            allPlayer.Remove(_ply);
            Debug.Log("Remove Ply: " + _ply);
        }
        if (allPlayer.Count < 2)
        {
            roominfo.roundInfo = roundInfo.none;
            Debug.LogError("Fatal Error Round Load Stoped");
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        roominfo.roundInfo = roundInfo.play;
        //stream.WhyUpdate(0);
        StreamSendData(StreamDataType.Players);
        //stream.WhyUpdate(1);
        StreamSendData(StreamDataType.Map);
        Pv.RPC("StartRound", RpcTarget.All);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        if (roominfo.roundInfo == roundInfo.play)
        {
            PhotonView _pv = PhotonView.Find(allPlayer[otherPlayer.ActorNumber].var.palyerGOId);
            _pv.TransferOwnership(PhotonNetwork.LocalPlayer);
            PhotonNetwork.Destroy(_pv.gameObject);
        }
        allPlayer.Remove(otherPlayer.ActorNumber);
        //stream.WhyUpdate(0);
        StreamSendData(StreamDataType.Players);
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
        Debug.Log(new Vector2(_x, _y) + "  " + blocks.ContainsKey(new Vector2(_x, _y)) + "  " + blocks.Count + "   " + PhotonNetwork.IsMasterClient);
        if (blocks[new Vector2(_x, _y)].state == BlockState.brekable)
        {
            blocks[new Vector2(_x, _y)].state = BlockState.destroyer;
        }
        //stream.WhyUpdate(1);
        StreamSendData(StreamDataType.Map);
    }

    [PunRPC]
    void TakePowerUp(int _why, int _what)
    {
        PowerUps _powerUp = (PowerUps)_what;
        allPlayer[_why].var.powerUps[(PowerUps)_what] = (int)allPlayer[_why].var.powerUps[(PowerUps)_what] + 1;
        Debug.LogFormat("Power Up: Speed: {0}, riadius: {1}, bombe:{2}", allPlayer[_why].var.powerUps[PowerUps.speed], allPlayer[_why].var.powerUps[PowerUps.moreRiadusse], allPlayer[_why].var.powerUps[PowerUps.moreBombe]);
        StreamSendData(StreamDataType.Players);
    }
    [PunRPC]
    void DestroyPower(int x, int y)
    {
        blocks[new Vector2(x, y)].PowerUp = 0;
        //stream.WhyUpdate(1);
        StreamSendData(StreamDataType.Map);
    }

    [PunRPC]
    void PlayerAlive(int _view, bool _alive)
    {
        allPlayer[_view].var.alive = _alive;
        //stream.WhyUpdate(0);
        StreamSendData(StreamDataType.Players);
    }

    [PunRPC]
    void PlySendProfil(string _name, int _hat, int _IdPly)
    {
        if (!allPlayer.ContainsKey(_IdPly))
        {
            PlayerVar ply = new PlayerVar();
            ply.name = _name;
            ply.hat = _hat;
            ply.color = Color.HSVToRGB(Random.Range(0f, 1f), 1f, 1f);
            allPlayer.Add(_IdPly, new PlayerData(ply));
            Debug.LogFormat("<color=green> Player Connect Name: {0}, hat: {1}, id: {2} </color>", _name, _hat, _IdPly);
        }
        //stream.WhyUpdate(0);
        StreamSendData(StreamDataType.Players);
    }

    [PunRPC]
    void SpawnHere(int _x, int _y)
    {
        Vector2Int _spawnPos = new Vector2Int(_x, _y);
        Vector2Int[] _blockDestroy = { _spawnPos + Vector2Int.up, _spawnPos + Vector2Int.down, _spawnPos + Vector2Int.left, _spawnPos + Vector2Int.right, _spawnPos };
        foreach (Vector2Int _posBlockDestroy in _blockDestroy)
        {
            if (blocks[_posBlockDestroy].state == BlockState.brekable)
            {
                blocks[_posBlockDestroy].state = BlockState.destroyer;
                blocks[_posBlockDestroy].PowerUp = 0;
            }
            
        }
        //stream.WhyUpdate(1);
        StreamSendData(StreamDataType.Map);
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

        if (!_bombeHere && allPlayer[_owner].var.BombeCount < (int)allPlayer[_owner].var.powerUps[PowerUps.moreBombe]+1)
        {
            GameObject _go = PhotonNetwork.Instantiate(bombe.name, _bombePos, Quaternion.identity);
            allPlayer[_owner].var.BombeCount += 1;
            StartCoroutine(TimerBombe(_go, _owner));
            //stream.WhyUpdate(0);
            StreamSendData(StreamDataType.Players);
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
        allPlayer[_owner].var.BombeCount -= 1;
        foreach (Vector2Int _dir in _alldir)
        {
            for (int i = 1; i <= ((int)allPlayer[_owner].var.powerUps[PowerUps.moreRiadusse] + 1); i++)
            {
                Vector2Int _expPos = new Vector2Int(_bombPos.x, _bombPos.y) + (new Vector2Int(_dir.x, _dir.y) * i);
                PhotonNetwork.Instantiate(explosion.name, new Vector3(_expPos.x + 0.5f, 0.3f, _expPos.y + 0.5f), Quaternion.identity);
                ExploseHere(_expPos, _owner);
                if (blocks[_expPos].state != BlockState.destroyer)
                {
                    Pv.RPC("DestroyBlock", RpcTarget.MasterClient, _expPos.x, _expPos.y);
                    break;
                }
            }
        }
        PhotonNetwork.Destroy(_bombe);
        //stream.WhyUpdate(1);
        StreamSendData(StreamDataType.Map);
        //stream.WhyUpdate(0);
        StreamSendData(StreamDataType.Players);
    }

    private void ExploseHere(Vector2 pose, int _owner)
    {
        foreach(KeyValuePair<int, PlayerData> _ply in allPlayer)
        {
            if (_ply.Value.var.alive)
            {
                Vector2Int _plypose = BM.Vec3To2int(PhotonView.Find(_ply.Value.var.palyerGOId).gameObject.transform.position);
                if (_plypose == pose)
                {
                    allPlayer[_ply.Key].var.alive = false;
                    //PhotonView.Find(_ply.Value.palyerGOId).RequestOwnership();
                    //PhotonNetwork.Destroy(PhotonView.Find(_ply.Value.palyerGOId).gameObject);
                    StartCoroutine(KillPlayer(_ply.Value.var.palyerGOId));
                    if (_ply.Key != _owner)
                    {
                        allPlayer[_owner].var.kill += 1;
                    }
                    else
                    {
                        allPlayer[_owner].var.kill -= 1;
                    }

                }
            }
                
        }
        //stream.WhyUpdate(0);
        StreamSendData(StreamDataType.Players);
    }

    IEnumerator KillPlayer(int _ply)
    {
        PhotonView.Find(_ply).RequestOwnership();
        while(PhotonView.Find(_ply).Owner != PhotonNetwork.LocalPlayer)
        {
            yield return new WaitForSeconds(0.01f); 
        }
        allPlayer[PhotonView.Find(_ply).Owner.ActorNumber].var.palyerGOId = -1;
        PhotonNetwork.Destroy(PhotonView.Find(_ply).gameObject);
        Pv.RPC("PlayerKilled", RpcTarget.All, _ply);
        yield return null;
    }

    [PunRPC]
    void DebugOnMaster(string _msg)
    {
        Debug.Log("<color=blue> " + _msg + "</color>");
    }
    public void StreamSendData(StreamDataType _type)
    {
        switch (_type)
        {
            case StreamDataType.Map:
                Dictionary<Vector2, string> _mapJson = new Dictionary<Vector2, string>();
                foreach (KeyValuePair<Vector2, BlockClass> _ply in blocks)
                {
                    _mapJson.Add(_ply.Key, JsonConvert.SerializeObject(_ply.Value));
                }
                stream.SendData(StreamDataType.Map, JsonConvert.SerializeObject(_mapJson));
                break;
            case StreamDataType.Players:
                Dictionary<int, string> _plysJson = new Dictionary<int, string>();
                foreach(KeyValuePair<int, PlayerData> _ply in allPlayer)
                {
                    allPlayer[_ply.Key].var.powerUpsJson = JsonConvert.SerializeObject(allPlayer[_ply.Key].var.powerUps);
                    _plysJson.Add(_ply.Key, JsonConvert.SerializeObject(_ply.Value.var));
                }
                stream.SendData(StreamDataType.Players, JsonConvert.SerializeObject(_plysJson));
                break;
        }
    }

}

public class PlayerData
{
    public PlayerVar var = new PlayerVar();

    public PlayerData()
    {
        ClassToOrigine();
    }
    public PlayerData(PlayerVar _var)
    {
        var = _var;
        ClassToOrigine();
    }
    public void ClassToOrigine()
    {
        var.powerUps = new Dictionary<PowerUps, int>();
        var.powerUps.Add(PowerUps.moreBombe,0);
        var.powerUps.Add(PowerUps.moreRiadusse,0);
        var.powerUps.Add(PowerUps.speed,0);
        var.BombeCount = 0;
        var.alive = false;
    }
}

public struct PlayerVar
{
    public string name;
    public int kill;
    public int win;
    public int hat;
    public int palyerGOId;
    public Color32 color;
    [JsonIgnore]
    public Dictionary<PowerUps, int> powerUps;
    public string powerUpsJson;
    public int BombeCount;
    public bool bot;
    public bool alive;
}
public enum GameModes
{
    classic
}
public class RoomInfoClass
{
    public int mapSize = 16;
    public float powerDensity = 0.2f;
    public GameModes gameMode = GameModes.classic;
    public roundInfo roundInfo = roundInfo.none;
    public bool debugRound;
}
public enum roundInfo
{
    none, play, end, preEnd, load
}
