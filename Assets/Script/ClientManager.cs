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
    public EndMenu endMenu;
    public StreamManager sManag;
    public static ClientManager client;
    public PhotonView Pv;
    public GameObject mainCam;
    public Transform allBlock;
    public Button roomSetBut;

    public Stat stat = new Stat();
    public Datas dataPlayer;
    public GameObject pauseMenu;


    void Start()
    {
        client = GetComponent<ClientManager>();
        InvokeRepeating("SetCountPlayer", 0f, 1f);
        dataPlayer = GetComponent<DataManager>().data;
        endMenu.gameObject.SetActive(false);
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

        if (!PhotonNetwork.InRoom)
        {
            SceneManager.LoadSceneAsync(0);
        }
        mainCam.SetActive( Myplayer == null);
        if (Myplayer != null)
        {
            playerPos = new Vector2Int(Mathf.FloorToInt(Myplayer.transform.position.x), Mathf.FloorToInt(Myplayer.transform.position.z));
        }
        if (Input.GetKey(KeyCode.F2))
        {
            try
            {
                photonInfo.text = string.Format("Player Count: {0}, RoomName: {1}, Clien ID: {2}, Master Client : {3}, Ping : {4}, Connection Stat : {5} ", 
                    playerCount.ToString(), 
                    PhotonNetwork.CurrentRoom.Name, 
                    PhotonNetwork.LocalPlayer.ActorNumber, 
                    allPlayer[PhotonNetwork.MasterClient.ActorNumber].var.name, 
                    PhotonNetwork.GetPing(),
                    PhotonNetwork.NetworkClientState);
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
            ValideConnection();
        }

        if (Input.GetKeyDown(KeyCode.F4))
        {
            Pv.RPC("DebugOnMaster", RpcTarget.MasterClient, "Client to master");
        }
        if (Input.GetKeyDown(KeyCode.F7))
        {
            CreateCubes();
        }
        if (Input.GetKeyDown(KeyCode.F8))
        {
            string _sp = "";
            foreach(KeyValuePair<int, PlayerData> _ply in allPlayer)
            {
                _sp += string.Format("Ply: {0} Name: {1} \n", _ply.Key, _ply.Value.var.name);
            }
            Debug.Log(_sp);
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
                CreateCubes();
                break;
            case StreamDataType.Players:
                //Debug.Log(_dataJson);
                Dictionary<int, string> _plysJson = (Dictionary<int, string>)JsonConvert.DeserializeObject(_dataJson, typeof(Dictionary<int, string>));
                allPlayer.Clear();
                foreach (KeyValuePair<int, string> _v in _plysJson)
                {
                    allPlayer.Add(_v.Key, new PlayerData((PlayerVar)JsonConvert.DeserializeObject(_v.Value, typeof(PlayerVar))));
                }
                break;
        }
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
        endMenu.gameObject.SetActive(false);
        CreateCubes();
        try
        {
            Myplayer = PhotonView.Find(allPlayer[PhotonNetwork.LocalPlayer.ActorNumber].var.palyerGOId).gameObject;
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
        Myplayer = PhotonNetwork.Instantiate(Pl.name, new Vector3(_spawnPos.x + 0.5f, 0.5f, _spawnPos.y + 0.5f), Quaternion.identity);
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
    void PlayerKilled(int _id)
    {
        if (Myplayer.GetPhotonView().ViewID == _id)
        {
            GetComponent<Spectator>().enabled = true;
        }
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
        endMenu.gameObject.SetActive(true);
        if (_winer != -1)
        {
            endMenu.winer = allPlayer[_winer].var.name;
            Debug.Log(allPlayer[_winer].var.name + " Won");
        }
        else
        {
            endMenu.winer = "No Player";
            Debug.Log("No Player Won");
        }
        endMenu.EndGame();
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
