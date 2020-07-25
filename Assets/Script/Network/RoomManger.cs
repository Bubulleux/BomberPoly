using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Newtonsoft.Json;
using System.Reflection;

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

    public static Dictionary<int,Client> allPlayer = new Dictionary<int, Client>();
    public Box[,] map;
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

    /*
     * F1: Debug mode
     * F2: Debug Info (cl)
     * F3: Lanche Game
     * F4: PrintPlyStat (cl/sv)
     * F5: Restart Cube (cl)
     * F6: Print Room State
     * F7: Shrinking map
     * F8: Clear Data Room
     * F9: Time * 5
     * F10: set all players alive (cl)
     * F11: force sync
     * F12:
     */

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
            foreach (KeyValuePair<int, Client> _v in allPlayer)
            {
                if (_v.Value.alive)
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
                    allPlayer[_winer].win++;
                }
                roominfo.roundInfo = roundInfo.preEnd;
                StartCoroutine(EndRound(_winer));
            }
            roominfo.cooldown -= Time.deltaTime * (Input.GetKey(KeyCode.F9) ? 5 : 1);
            if (roominfo.cooldown <= 0f)
            {
                roominfo.shrinking++;
                MapShrinking(roominfo.shrinking);
                roominfo.cooldown = roominfo.timeBetweenShrinking;
            }
        }
        //stream.allPlayer = allPlayer;

        if (Input.GetKeyDown(KeyCode.F6))
        {
            Debug.LogFormat("Room State {0}, Count Player {1}",
                roominfo.roundInfo,
                allPlayer.Count);
        }
        if (Input.GetKeyDown(KeyCode.F8))
        {
            ClearDataRoom();
        }
        if (roominfo.debugRound && Input.GetKeyDown(KeyCode.F7))
        {
            MapShrinking(2);
        }
        StreamSendData(StreamDataType.Room);
        if (Input.GetKeyDown(KeyCode.F4))
        {
            foreach (KeyValuePair<int, Client> _ply in allPlayer)
            {
                Debug.LogFormat("Ply: {0}, Name {1}, ping {2}, GO Id {3},  bot {4} , alive {4} (sv)", _ply.Key, _ply.Value.name, _ply.Value.ping, _ply.Value.plyInstancePvId, _ply.Value.bot, _ply.Value.alive);
            }
        }
        if (Input.GetKeyDown(KeyCode.F11))
        {
            AllSync();
        }
    }
    void AllSync()
    {
        StreamSendData(StreamDataType.Map);
        StreamSendData(StreamDataType.Players);
        StreamSendData(StreamDataType.Room);
    }

    public void ClearDataRoom()
    {
        RoomManagerCom = this;
        roominfo = new RoomInfoClass();
        map = null;
        ClearScene(roundInfo.none);
        Debug.Log("<color=red> Room's Data was clear </color>");
    }

    public IEnumerator EndRound(int _winer)
    {
        
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
        foreach (KeyValuePair<int, Client> _cl in allPlayer)
        {
            allPlayer[_cl.Key].alive = false;
            allPlayer[_cl.Key].plyInstancePvId = -1;
        }
        StreamSendData(StreamDataType.Players);
    }
    IEnumerator StartRound(bool _debug)
    {
        if (roominfo.roundInfo != roundInfo.play && roominfo.roundInfo != roundInfo.none)
        {
            yield break;
        }
        ClearScene(roundInfo.load);
        Debug.LogFormat("<color=blue> {0} Round Start </color>", _debug ? "Debug" :"");
        roominfo.cooldown = roominfo.timeBeforeShrinking;
        roominfo.shrinking = 0;
        foreach (GameObject _go in GameObject.FindGameObjectsWithTag("Player"))
        {
            //_go.GetPhotonView().RequestOwnership();
            //PhotonNetwork.Destroy(_go);
            StartCoroutine(KillPlayer(_go.GetPhotonView().ViewID));
        }
        if (allPlayer.Count < 2)
        {
            int _plyInt = -1;
            Client _plyData = new Client()
            {
                
                name = "Bot",
                color = Color.white,
                bot = true
            };
            try
            {
                allPlayer.Add(_plyInt, _plyData);
            }
            catch { }
        }
        map = Map.GenerMap(roominfo.mapSize, roominfo);
        yield return new WaitForSeconds(2f);
        List<int> _remove = new List<int>();
        foreach (KeyValuePair<int, Client> _ply in allPlayer)
        {
            //Debug.Log(_ply.Key);
            if (IsFind(_ply.Key) || (_ply.Value.bot && _debug && allPlayer.Count < 3))
            {
                allPlayer[_ply.Key].ClassToOrigine();
                Vector2Int _spawnPos = new Vector2Int(Random.Range(1, 8) * 2 - 1, Random.Range(1, 8) * 2 - 1);
                MakeHole(_spawnPos.x, _spawnPos.y);
                GameObject _player = PhotonNetwork.Instantiate(plyGO.name, new Vector3(_spawnPos.x + 0.5f, 0.5f, _spawnPos.y + 0.5f), Quaternion.identity);
                _player.GetPhotonView().TransferOwnership(_ply.Key);
                allPlayer[_ply.Key].alive = true;
                allPlayer[_ply.Key].plyInstancePvId = _player.GetPhotonView().ViewID;
                if (_ply.Value.bot)
                {
                    _player.GetComponent<PlayerControlerCl>().enabled = false;
                    _player.GetComponent<PlayerControlerCl>().cam.SetActive(false);
                }
            }
            else
            {
                _remove.Add(_ply.Key);
            }
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
        
        StreamSendData(StreamDataType.Players);
        
        StreamSendData(StreamDataType.Map);
        Pv.RPC("StartRound", RpcTarget.All);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        if (roominfo.roundInfo == roundInfo.play)
        {
            PhotonView _pv = PhotonView.Find(allPlayer[otherPlayer.ActorNumber].plyInstancePvId);
            _pv.TransferOwnership(PhotonNetwork.LocalPlayer);
            PhotonNetwork.Destroy(_pv.gameObject);
        }
        allPlayer.Remove(otherPlayer.ActorNumber);
        
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

    public void TakePowerUp(Client _cl , PowerUps _powerUp)
    {
        if (_powerUp == PowerUps.mistery && _cl.GetPly().mysteryPower == MysteryPower.MysteryPowers.none)
        {
            _cl.GetPly().mysteryPower = (MysteryPower.MysteryPowers)Random.Range(1, 3);
        }
        else if (_powerUp != PowerUps.mistery)
        {
            _cl.GetPly().powerUps[_powerUp] = (int)_cl.GetPly().powerUps[_powerUp] + 1;
            Debug.LogFormat("Power Up: Speed: {0}, riadius: {1}, bombe:{2}", _cl.GetPly().powerUps[PowerUps.speed], _cl.GetPly().powerUps[PowerUps.moreRiadusse], _cl.GetPly().powerUps[PowerUps.moreBombe]);
        }
        StreamSendData(StreamDataType.Players);
    }
    [PunRPC]
    void DestroyPower(int x, int y)
    {
        map[x,y].PowerUp = 0;
        
        StreamSendData(StreamDataType.Map);
    }

    [PunRPC]
    void PlayerAlive(int _view, bool _alive)
    {
        allPlayer[_view].alive = _alive;
        
        StreamSendData(StreamDataType.Players);
        Debug.LogFormat("Ply {0} alive: {1}", _view, _alive);
    }

    [PunRPC]
    void PlySendProfil(string _name, int _hat, int _IdPly)
    {
        if (!allPlayer.ContainsKey(_IdPly))
        {
            Client ply = new Client();
            ply.name = _name;
            ply.hat = _hat;
            ply.color = Color.HSVToRGB(Random.Range(0f, 1f), 1f, 1f);
            allPlayer.Add(_IdPly, ply);
            Debug.LogFormat("<color=green> Player Connect Name: {0}, hat: {1}, id: {2} </color>", _name, _hat, _IdPly);
            AllSync();
        }
    }

    public void MakeHole(int _x, int _y)
    {
        Vector2Int _spawnPos = new Vector2Int(_x, _y);
        Vector2Int[] _blockDestroy = { _spawnPos + Vector2Int.up, _spawnPos + Vector2Int.down, _spawnPos + Vector2Int.left, _spawnPos + Vector2Int.right, _spawnPos };
        foreach (Vector2Int _posBlockDestroy in _blockDestroy)
        {
            if (map[_posBlockDestroy.x, _posBlockDestroy.y].state == BlockState.brekable)
            {
                map[_posBlockDestroy.x, _posBlockDestroy.y].state = BlockState.destroyer;
                map[_posBlockDestroy.x, _posBlockDestroy.y].PowerUp = 0;
            }

        }

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

        if (!_bombeHere && allPlayer[_owner].GetPly().BombeCount < (int)allPlayer[_owner].GetPly().powerUps[PowerUps.moreBombe]+1)
        {
            
            StartCoroutine(TimerBombe(_bombePos,_owner, false));
        }
    }
    public IEnumerator TimerBombe(Vector3 _bombePos, int _owner, bool _megaBombe)
    {
        GameObject _go = PhotonNetwork.Instantiate(bombe.name, _bombePos, Quaternion.identity);
        allPlayer[_owner].GetPly().BombeCount += 1;
        StreamSendData(StreamDataType.Players);
        yield return new WaitForSeconds(3f);
        Explositon(_go, _owner, _megaBombe);
    }

    public void Explositon( GameObject _bombe, int _owner, bool _megaBombe)
    {
        Vector3 _pos = _bombe.transform.position;
        PhotonNetwork.Instantiate(explosion.name, _pos + new Vector3(0.5f, 0.3f, 0.5f), Quaternion.identity);
        Vector2Int _bombPos = new Vector2Int(Mathf.FloorToInt(_pos.x), Mathf.FloorToInt(_pos.z));
        ExploseHere(_bombPos, _owner);
        Vector2Int[] _alldir = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        if (allPlayer[_owner].alive)
        {
            allPlayer[_owner].GetPly().BombeCount -= 1;
        }
        int _rad = ((int)allPlayer[_owner].GetPly().powerUps[PowerUps.moreRiadusse] + 1);
        if (_megaBombe)
        {
            _rad = 999;
        }
        foreach (Vector2Int _dir in _alldir)
        {
            for (int i = 1; i <= _rad; i++)
            {
                Vector2Int _expPos = new Vector2Int(_bombPos.x, _bombPos.y) + (new Vector2Int(_dir.x, _dir.y) * i);
                int x = _bombPos.x + _dir.x * i;
                int y = _bombPos.y + _dir.y * i;
                PhotonNetwork.Instantiate(explosion.name, new Vector3(_expPos.x + 0.5f, 0.3f, _expPos.y + 0.5f), Quaternion.identity);
                ExploseHere(_expPos, _owner);
                if (map[x,y].state != BlockState.destroyer)
                {
                    if (map[x, y].state == BlockState.brekable)
                    {
                        map[x, y].state = BlockState.destroyer;
                    }
                    if (!_megaBombe || map[x, y].state == BlockState.unbrekable)
                    {
                        break;
                    }
                }
            }
        }
        PhotonNetwork.Destroy(_bombe);
        
        StreamSendData(StreamDataType.Map);
        
        StreamSendData(StreamDataType.Players);
    }

    private void ExploseHere(Vector2 pose, int _owner)
    {
        foreach(KeyValuePair<int, Client> _ply in allPlayer)
        {
            if (_ply.Value.alive)
            {
                Vector2Int _plypose = BM.Vec3To2int(PhotonView.Find(_ply.Value.plyInstancePvId).gameObject.transform.position);
                if (_plypose == pose)
                {
                    allPlayer[_ply.Key].alive = false;
                    //PhotonView.Find(_ply.Value.plyInstancePvId).RequestOwnership();
                    //PhotonNetwork.Destroy(PhotonView.Find(_ply.Value.plyInstancePvId).gameObject);
                    StartCoroutine(KillPlayer(_ply.Value.plyInstancePvId));
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
        
        StreamSendData(StreamDataType.Players);
    }

    IEnumerator KillPlayer(int _ply)
    {
        allPlayer[PhotonView.Find(_ply).OwnerActorNr].alive = false;
        PhotonView.Find(_ply).RequestOwnership();
        while(PhotonView.Find(_ply).Owner != PhotonNetwork.LocalPlayer)
        {
            yield return new WaitForSeconds(0.01f); 
        }
        allPlayer[PhotonView.Find(_ply).Owner.ActorNumber].plyInstancePvId = -1;
        PhotonNetwork.Destroy(PhotonView.Find(_ply).gameObject);
        //Pv.RPC("PlayerKilled", RpcTarget.All, _ply);
        yield return null;
    }

    void MapShrinking(int _shrinking)
    {
        for(int x = 0; x <= roominfo.mapSize; x++)
        {
            for (int y = 0; y <= roominfo.mapSize; y++)
            {
                bool _xAct = x <= _shrinking || x >= roominfo.mapSize - _shrinking;
                bool _yAct = y <= _shrinking || y >= roominfo.mapSize - _shrinking;
                if (_xAct || _yAct)
                {
                    map[x,y].state = BlockState.unbrekable;
                    foreach(KeyValuePair<int, Client> _ply in allPlayer)
                    {
                        if (_ply.Value.alive)
                        {
                            bool _die = BM.Vec3To2int(PhotonView.Find(_ply.Value.plyInstancePvId).transform.position) == new Vector2Int(x, y);
                            if (_die)
                            {
                                StartCoroutine(KillPlayer(_ply.Value.plyInstancePvId));
                            }
                        }
                    }
                }
            }
        }
        StreamSendData(StreamDataType.Map);
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
                if (map == null)
                {
                    break;
                }
                string[,] _mapJson = new string[map.GetLength(0), map.GetLength(1)];
                for (int x = 0; x < map.GetLength(0); x++)
                {
                    for (int y = 0; y < map.GetLength(1); y++)
                    {
                        _mapJson[x,y] = JsonConvert.SerializeObject(map[x,y]);
                    }
                }
                stream.SendData(StreamDataType.Map, JsonConvert.SerializeObject(_mapJson));
                break;
            case StreamDataType.Players:
                Dictionary<int, string> _plysJson = new Dictionary<int, string>();
                foreach(KeyValuePair<int, Client> _ply in allPlayer)
                {
                    _plysJson.Add(_ply.Key, _ply.Value.json());
                }
                stream.SendData(StreamDataType.Players, JsonConvert.SerializeObject(_plysJson));
                break;
            case StreamDataType.Room:
                stream.SendData(StreamDataType.Room, JsonConvert.SerializeObject(roominfo));
                break;
        }
    }
    [PunRPC]
    void SendPing(int _ply, int _ping)
    {
        allPlayer[_ply].ping = _ping;
        StreamSendData(StreamDataType.Players);
    }
    
}
public enum GameModes
{
    classic
}
public class RoomInfoClass
{
    public int mapSize = 20;
    public int timeBeforeShrinking = 90;
    public int timeBetweenShrinking = 15;
    public float powerDensity = 0.2f;
    public float mysteryPowerDensity = 0.15f;
    public float boxDensity = 0.9f;
    public bool safeTp = true;
    public GameModes gameMode = GameModes.classic;
    public roundInfo roundInfo = roundInfo.none;
    public bool debugRound;
    public float cooldown = 0f;
    public int shrinking = 0;
}
public enum roundInfo
{
    none, play, end, preEnd, load
}
