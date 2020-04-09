using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
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
        
        Pv.RPC("PlayerConnecte", RpcTarget.MasterClient, dataPlayer.name, dataPlayer.hat, PhotonNetwork.LocalPlayer.ActorNumber);
        sManag = GameObject.FindGameObjectWithTag("Stream").GetComponent<StreamManager>();

        
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
                    allPlayer[PhotonNetwork.MasterClient.ActorNumber].name, 
                    PhotonNetwork.GetPing(),
                    PhotonNetwork.NetworkClientState);
            }
            catch
            {
                photonInfo.text = "Wait";
            }
        }
        else
        {
            photonInfo.text = null;
        }

        try
        {
            allPlayer = sManag.allPlayer;
        }
        catch
        {

        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            pauseMenu.SetActive(!pauseMenu.activeSelf);
            roomSetBut.interactable = PhotonNetwork.IsMasterClient;
        }
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        CreateCubes();
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
        base.OnMasterClientSwitched(newMasterClient);
        Disconect("Master Client Switched");
    }

    public void Disconect(string _m)
    {
        PhotonNetwork.LeaveRoom();
        Debug.LogWarning(_m);
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
        Debug.Log(Blocks.Count);
        string p = "";
        foreach(KeyValuePair<Vector2, BlockClass> _block in Blocks)
        {
            Instantiate(block, new Vector3(_block.Key.x, 0f, _block.Key.y), Quaternion.identity, allBlock);
            p += string.Format("Block in x:{0} y:{1} has benne create \n", _block.Key.x, _block.Key.y);
        }
        //Debug.Log(p);
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
            Myplayer = PhotonView.Find(allPlayer[PhotonNetwork.LocalPlayer.ActorNumber].palyerGOId).gameObject;
        }
        catch
        {
            Debug.LogErrorFormat("PhotonView: {0} not find", allPlayer[PhotonNetwork.LocalPlayer.ActorNumber].palyerGOId);
        }
        
    }

    [PunRPC]
    void CreatPlayer()
    {
        stat = new Stat();
        Vector2Int _spawnPos = new Vector2Int(Random.Range(1, 8) * 2 - 1, Random.Range(1, 8) * 2 - 1);
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
            endMenu.winer = allPlayer[_winer].name;
            Debug.Log(allPlayer[_winer].name + " Won");
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
