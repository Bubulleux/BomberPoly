using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Newtonsoft.Json;
using System;
public class ClientManager : MonoBehaviourPunCallbacks
{
    public GameObject Pl;
    public GameObject block;

    public GameObject Myplayer;

    public int playerCount;
    private float cooldown;
    public Vector2Int playerPos;

    public roundInfo roundStat;


    public Dictionary<int, PlayerData> allPlayer = new Dictionary<int, PlayerData>();
    //public IDictionary<Vector2Int, GameObject> allBlockInDict;
    public Dictionary<Vector2, BlockClass> Blocks = new Dictionary<Vector2, BlockClass>();

    public Text photonInfo;
    public StreamManager sManag;
    public static ClientManager client;
    public PhotonView Pv;
    public GameObject mainCam;
    public Transform allBlock;
    public Button roomSetBut;

    public Stat stat = new Stat();
    public Datas dataPlayer;
    public GameObject pauseMenu;
    public RoomInfoClass roomInfo;

    public Scoretable scoreTable;


    void Start()
    {
        client = GetComponent<ClientManager>();
        InvokeRepeating("SetCountPlayer", 0f, 1f);
        InvokeRepeating("SendPing", 0f, 5f);
        dataPlayer = GetComponent<DataManager>().data;
        ValideConnection();
        //sManag = GameObject.FindGameObjectWithTag("Stream").GetComponent<StreamManager>();
        //Debug.developerConsoleVisible = true;
    }

    void Update()
    {
        if (sManag == null)
        {
            try
            {
                sManag = GameObject.FindGameObjectWithTag("Stream").GetComponent<StreamManager>();
            }
            catch
            {}            
        }
        if (Input.GetKeyDown(KeyCode.F4))
        {
            foreach (KeyValuePair<int, PlayerData> _ply in allPlayer)
            {
                Debug.LogFormat("Ply: {0}, Name {1}, ping {2}, GO Id {3},  bot {4} , alive {4} (sv)", _ply.Key, _ply.Value.var.name, _ply.Value.var.ping, _ply.Value.var.palyerGOId, _ply.Value.var.bot, _ply.Value.var.alive);
            }
        }

        if (!PhotonNetwork.InRoom)
        {
            SceneManager.LoadSceneAsync(1);
        }
        mainCam.SetActive( Myplayer == null);
        if (Myplayer != null && roomInfo.roundInfo == roundInfo.play)
        {
            try
            {
                Myplayer.GetComponent<PlayerGo>().enabled = true;
                playerPos = new Vector2Int(Mathf.FloorToInt(Myplayer.transform.position.x), Mathf.FloorToInt(Myplayer.transform.position.z));
                Myplayer.GetComponent<PlayerGo>().cam.SetActive(true);
            }
            catch
            {
                Myplayer = PhotonView.Find(allPlayer[PhotonNetwork.LocalPlayer.ActorNumber].var.palyerGOId).gameObject;
            }
        }
        if (Myplayer == null && roomInfo.roundInfo == roundInfo.play && LocalPly().var.alive)
        {
            FindMyPly();
        }
        if (Input.GetKey(KeyCode.F2))
        {
            try
            {
                photonInfo.text = string.Format("Player Count: {0}, RoomName: {1}, Clien ID: {2}, Master Client : {3}, Ping : {4} ms, Connection Stat : {5} , RoomStatus {6}", 
                    playerCount.ToString(), 
                    PhotonNetwork.CurrentRoom.Name, 
                    PhotonNetwork.LocalPlayer.ActorNumber, 
                    allPlayer[PhotonNetwork.MasterClient.ActorNumber].var.name, 
                    PhotonNetwork.GetPing(),
                    PhotonNetwork.NetworkClientState,
                    roomInfo.roundInfo.ToString());
            }
            catch (Exception e)
            {
                photonInfo.text = string.Format("Plese Wait...   Error: \"{0}\"    Connection Status: \"{1}\"", e.Message, PhotonNetwork.NetworkClientState);
            }
        }
        else
        {
            photonInfo.text = null;
        }
        try
        {
            //allPlayer = sManag.allPlayer;
        }
        catch
        {

        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            pauseMenu.SetActive(!pauseMenu.activeSelf);
            roomSetBut.interactable = PhotonNetwork.IsMasterClient;
        }
        if (Input.GetKeyDown(KeyCode.F5))
        {
            CreateCubes();
        }
       
        if (Input.GetKeyDown(KeyCode.F10))
        {
            allPlayer[1].var.alive = true;
        }
    }
    public void RecevingData(StreamDataType _type, string _dataJson)
    {
        switch (_type)
        {
            case StreamDataType.Map:
                Dictionary<Vector2, string> _mapJson = (Dictionary<Vector2, string>)JsonConvert.DeserializeObject(_dataJson, typeof(Dictionary<Vector2, string>));
                Blocks.Clear();
                foreach(KeyValuePair<Vector2, string> _v in _mapJson)
                {
                    Blocks.Add(_v.Key, (BlockClass)JsonConvert.DeserializeObject(_v.Value, typeof(BlockClass)));
                }
                //CreateCubes();
                break;
            case StreamDataType.Players:
                Dictionary<int, string> _plysJson = (Dictionary<int, string>)JsonConvert.DeserializeObject(_dataJson, typeof(Dictionary<int, string>));
                allPlayer.Clear();
                foreach (KeyValuePair<int, string> _v in _plysJson)
                {
                    allPlayer.Add(_v.Key, new PlayerData((PlayerVar)JsonConvert.DeserializeObject(_v.Value, typeof(PlayerVar))));
                    allPlayer[_v.Key].var.powerUps = (Dictionary<PowerUps, int>)JsonConvert.DeserializeObject(allPlayer[_v.Key].var.powerUpsJson, typeof(Dictionary<PowerUps, int>));
                }
                scoreTable.InitialazePlayerTable();
                break;
            case StreamDataType.Room:
                roomInfo = (RoomInfoClass)JsonConvert.DeserializeObject(_dataJson, typeof(RoomInfoClass));
                break;
                
        }
    }
    void SendPing()
    {
        Pv.RPC("SendPing", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber, PhotonNetwork.GetPing());
    }

    public override void OnJoinedRoom()
    {
        CreateCubes();
    }

    private void ValideConnection()
    {
        Pv.RPC("PlySendProfil", RpcTarget.MasterClient, dataPlayer.name, dataPlayer.hat, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    public void MakeBlock(bool _unbreakabel, bool _wall, Vector2Int _pos)
    {
        Pv.RPC("MakeMyBlock", RpcTarget.All, _unbreakabel, _wall, _pos.x, _pos.y);
    }

    private void SetCountPlayer()
    {
        playerCount = allPlayer.Count;
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Disconect("Master Client Switched");
    }

    public void Disconect(string _m)
    {
        PhotonNetwork.LeaveRoom();
        Debug.LogFormat("<color=red> {0} </color>", _m);
    }
    public void CreateCubes()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Blocks = RoomManger.RoomManagerCom.blocks;
        }
        foreach(GameObject go in GameObject.FindGameObjectsWithTag("Block"))
        {
            Destroy(go);
        }
        foreach(KeyValuePair<Vector2, BlockClass> _block in Blocks)
        {
            Instantiate(block, new Vector3(_block.Key.x, 0f, _block.Key.y), Quaternion.identity, allBlock);
        }
    }


    [PunRPC]
    public void DestroyBlock()
    {
        //allBlockInDict = new Dictionary<Vector2Int, GameObject>();
        foreach (GameObject _block in GameObject.FindGameObjectsWithTag("Block"))
        {
            Destroy(_block);
        }
        Pv.RPC("PlayerAlive", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber, false);
    }
    [PunRPC]
    void StartRound()
    {
        scoreTable.enabelScoreBorad = false;
        CreateCubes();
    }
    void FindMyPly()
    {
        try
        {
            Myplayer = PhotonView.Find(allPlayer[PhotonNetwork.LocalPlayer.ActorNumber].var.palyerGOId).gameObject;
            Debug.Log("MyPly : " + Myplayer + "    " + allPlayer[PhotonNetwork.LocalPlayer.ActorNumber].var.palyerGOId);
        }
        catch
        {
            Debug.LogErrorFormat("PhotonView: {0} not find", allPlayer[PhotonNetwork.LocalPlayer.ActorNumber].var.palyerGOId);
        }
    }

    [PunRPC]
    void CreatPlayer()
    {
        stat = new Stat();
        Vector2Int _spawnPos = new Vector2Int(UnityEngine.Random.Range(1, 8) * 2 - 1, UnityEngine. Random.Range(1, 8) * 2 - 1);
        Pv.RPC("SpawnHere", RpcTarget.All, _spawnPos.x, _spawnPos.y);
        //Myplayer = PhotonNetwork.Instantiate(Pl.name, new Vector3(_spawnPos.x + 0.5f, 0.5f, _spawnPos.y + 0.5f), Quaternion.identity);
        Pv.RPC("PlayerAlive", RpcTarget.MasterClient, Pv.ViewID, true);
        CreateCubes();
    }

    
    [PunRPC]
    void InitialasePowerUp(int _id, int _power)
    {
       PhotonView.Find(_id).GetComponent<PowerUp>().Initilase(_power);
    }

    [PunRPC]
    void DestroyPlayer()
    {
        PhotonNetwork.Destroy(Myplayer);
        Pv.RPC("PlayerAlive", RpcTarget.MasterClient, Pv.ViewID, false);
    }


    [PunRPC]
    void ExploseHere(int _x, int _y)
    {
        if (playerPos == new Vector2Int(_x, _y))
        {
            PhotonNetwork.Destroy(Myplayer);
            Pv.RPC("PlayerAlive", RpcTarget.MasterClient, Pv.ViewID, false);
        }
    }

    public void DestroyGameObj(GameObject _obj)
    {
        PhotonNetwork.Destroy(_obj);
    }

    [PunRPC]
    void RoundEnd(int _winer)
    {

    }
    [PunRPC]
    void ClearScene()
    {
        string[] tagDestroy = { "Block", "PowerUp", "Player" };
        foreach (string tag in tagDestroy)
        {
            foreach (GameObject go in GameObject.FindGameObjectsWithTag(tag))
            {
                Destroy(go);
            }
        }
    }

    public PlayerData LocalPly()
    {
        return allPlayer[PhotonNetwork.LocalPlayer.ActorNumber];
    }
    

    //[PunRPC]
    //void TakePowerUp(int _id, int _power)
    //{
    //    if (_id == PhotonNetwork.LocalPlayer.ActorNumber)
    //    {
    //        if (_power == 1)
    //        {
    //            stat.explositonSize += 1;
    //        }
    //        if (_power == 2)
    //        {
    //            stat.speed += 0.25f;
    //        }
    //    }
        
    //}

    void UpdatePlayerCount(bool AddToCount)
    {
        playerCount = PhotonNetwork.CountOfRooms;
        Debug.Log("Count Player A been set");
    }
    

}
public class Stat
{
    public float speed = 2f;
    public int explositonSize = 1;
}
