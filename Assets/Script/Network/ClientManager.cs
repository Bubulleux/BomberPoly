using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Newtonsoft.Json;
using System;
using System.Reflection;
public class ClientManager : MonoBehaviourPunCallbacks
{
    public GameObject Pl;
    public GameObject block;

    public GameObject Myplayer;

    public int playerCount;
    private float cooldown;
    public Vector2Int playerPos;

    public roundInfo roundStat;


    public Dictionary<int, Client> allPlayer = new Dictionary<int, Client>();
    //public IDictionary<Vector2Int, GameObject> allBlockInDict;

    public Text photonInfo;
    public StreamManager sManag;
    public static ClientManager client;
    public PhotonView Pv;
    public GameObject mainCam;
    public Map map;
    public Button roomSetBut;

    public Stat stat = new Stat();
    public Datas dataPlayer;
    public GameObject pauseMenu;
    public RoomInfoClass roomInfo;

    public Scoretable scoreTable;

    private void Awake()
    {
        client = GetComponent<ClientManager>();

        if (!PhotonNetwork.InRoom)
        {
            SceneManager.LoadSceneAsync(1);
        }
    }
    void Start()
    {
        InvokeRepeating("SetCountPlayer", 0f, 1f);
        InvokeRepeating("SendPing", 0f, 5f);
        dataPlayer = DataManager.GetData();
        ValideConnection();
        //sManag = GameObject.FindGameObjectWithTag("Stream").GetComponent<StreamManager>();
        //Debug.developerConsoleVisible = true;
    }

    void Update()
    {
        if (!PhotonNetwork.InRoom)
        {
            SceneManager.LoadSceneAsync(1);
        }
        if (allPlayer.Count == 0)
        {
            return;
        }
        if (Input.GetKey(KeyCode.F2))
        {
            try
            {
                photonInfo.text = string.Format("Player Count: {0}, RoomName: {1}, Clien ID: {2}, Master Client : {3}, Ping : {4} ms, Connection Stat : {5} , RoomStatus {6}",
                    playerCount.ToString(),
                    PhotonNetwork.CurrentRoom.Name,
                    PhotonNetwork.LocalPlayer.ActorNumber,
                    allPlayer[PhotonNetwork.MasterClient.ActorNumber].name,
                    PhotonNetwork.GetPing(),
                    PhotonNetwork.NetworkClientState,
                    roomInfo.roundInfo.ToString());
            }
            catch (Exception e)
            {
                photonInfo.text = string.Format("Plese Wait...   Error: \"{0}\"    Connection Status: \"{1}\"", e.Message, PhotonNetwork.NetworkClientState);
            }
        }
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
            foreach (KeyValuePair<int, Client> _ply in allPlayer)
            {
                Debug.LogFormat("Ply: {0}, Name {1}, ping {2}, GO Id {3},  bot {4} , alive {4}", _ply.Key, _ply.Value.name, _ply.Value.ping, _ply.Value.plyInstancePvId, _ply.Value.bot, _ply.Value.alive);
            }
        }

        mainCam.SetActive( Myplayer == null);
        if (Myplayer != null && roomInfo.roundInfo == roundInfo.play)
        {
            try
            {
                Myplayer.GetComponent<PlayerControlerCl>().enabled = true;
                playerPos = new Vector2Int(Mathf.FloorToInt(Myplayer.transform.position.x), Mathf.FloorToInt(Myplayer.transform.position.z));
                Myplayer.GetComponent<PlayerControlerCl>().cam.SetActive(true);
            }
            catch
            {
                Myplayer = PhotonView.Find(allPlayer[PhotonNetwork.LocalPlayer.ActorNumber].plyInstancePvId).gameObject;
            }
        }
        if (Myplayer == null && roomInfo.roundInfo == roundInfo.play && LocalPly().alive)
        {
            FindMyPly();
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
            if (roomInfo.roundInfo == roundInfo.play && LocalPly().alive)
            {
                LocalPly().GetPly().GetComponent<PlayerControlerCl>().enabled = !pauseMenu.activeSelf;
            }
        }
        if (Input.GetKeyDown(KeyCode.F5))
        {
            map.RenderMap();
        }
       
        if (Input.GetKeyDown(KeyCode.F10))
        {
            allPlayer[1].alive = true;
        }
    }
    public void RecevingData(StreamDataType _type, string _dataJson)
    {
        switch (_type)
        {
            //case StreamDataType.Map:
            //    string[,] _mapJson = JsonConvert.DeserializeObject<string[,]>(_dataJson);
            //    map.Maps = new Box[_mapJson.GetLength(0), _mapJson.GetLength(0)];
            //    for (int x = 0; x < _mapJson.GetLength(0); x++)
            //    {
            //        for (int y = 0; y < _mapJson.GetLength(1); y++)
            //        {
            //            map.Maps[x,y] = JsonConvert.DeserializeObject<Box>(_mapJson[x,y]);
            //        }
            //    }
            //    map.UpdateMap();
            //    break;
            case StreamDataType.Players:
                Dictionary<int, string> _plysJson = (Dictionary<int, string>)JsonConvert.DeserializeObject(_dataJson, typeof(Dictionary<int, string>));
                allPlayer.Clear();
                foreach (KeyValuePair<int, string> _v in _plysJson)
                {
                    allPlayer.Add(_v.Key, JsonConvert.DeserializeObject<Client>(_v.Value));
                    //allPlayer[_v.Key].GetPly().powerUps = (Dictionary<PowerUps, int>)JsonConvert.DeserializeObject(allPlayer[_v.Key].GetPly().powerUpsJson, typeof(Dictionary<PowerUps, int>));
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
        map.RenderMap();
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
    public void ForceDisconect()
    {

    }

    
    [PunRPC]
    void StartRound()
    {
        scoreTable.enabelScoreBorad = false;
        map.RenderMap();
    }
    void FindMyPly()
    {
        try
        {
            Myplayer = PhotonView.Find(allPlayer[PhotonNetwork.LocalPlayer.ActorNumber].plyInstancePvId).gameObject;
        }
        catch
        {
            Debug.LogErrorFormat("PhotonView: {0} not find", allPlayer[PhotonNetwork.LocalPlayer.ActorNumber].plyInstancePvId);
        }
    }

    [PunRPC]
    void CreatPlayer()
    {
        stat = new Stat();
        Vector2Int _spawnPos = new Vector2Int(UnityEngine.Random.Range(1, 8) * 2 - 1, UnityEngine. Random.Range(1, 8) * 2 - 1);
        //Pv.RPC("SpawnHere", RpcTarget.All, _spawnPos.x, _spawnPos.y);
        //Myplayer = PhotonNetwork.Instantiate(Pl.name, new Vector3(_spawnPos.x + 0.5f, 0.5f, _spawnPos.y + 0.5f), Quaternion.identity);
        Pv.RPC("PlayerAlive", RpcTarget.MasterClient, Pv.ViewID, true);
        map.RenderMap();
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

    public Client LocalPly()
    {
        try
        {
            return allPlayer[PhotonNetwork.LocalPlayer.ActorNumber];
        }
        catch
        {
            return new Client();
        }
    }
    

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
public class Client
{
    public string name = "No Name";
    public int kill = 0;
    public int win = 0;
    public int hat = 0;
    public Color32 color;
    public bool bot;
    public int ping;
    public bool alive;
    public int plyInstancePvId = -1;

    public string json()
    {
        Dictionary<string, string> dicofVar = new Dictionary<string, string>();
        foreach (FieldInfo field in this.GetType().GetFields())
        {
            //string stringValue = Convert.ToBase64String(ObjectSerialize.Serialize(field.GetValue(this)));
            //dicofVar.Add(field.Name, stringValue);

        }
        
        return JsonConvert.SerializeObject(this);
    }
    public PlayerCl GetPly()
    {
        GameObject plyInstance = null;
        try
        {
            plyInstance = PhotonView.Find(plyInstancePvId).gameObject;
        }
        catch
        {}
        if (plyInstance != null)
        {
            return plyInstance.GetComponent<PlayerCl>();
        }
        return null;
    }
    public Client()
    {
        ClassToOrigine();
    }
    public void ClassToOrigine()
    {
        alive = false;
    }
}