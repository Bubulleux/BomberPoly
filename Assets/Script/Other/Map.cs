using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Newtonsoft.Json;

public class Map : MonoBehaviour, IPunObservable
{
    private List<Box> boxSync = new List<Box>();
    private Box[,] map;
    public Box[,] Maps { get { return map; } }

    public GameObject block;
    public void RenderMap()
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Block"))
        {
            Destroy(go);
        }
        for (int y = 0; y < map.GetLength(1); y++)
        {
            for (int x = 0; x < map.GetLength(0); x++)
            {
                Instantiate(block, new Vector3(x, 0f, y), Quaternion.identity, transform);
            }
        }
        UpdateMap();
    }
    public void UpdateMap()
    {
        UpdateMap(boxSync);
    }
    public void UpdateMap(List<Box> _boxSync)
    {
        foreach(Box box in _boxSync)
        {
            
            GameObject boxGo = transform.GetChild(i).gameObject;
            GameObject gfx = boxGo.transform.Find("GFX").gameObject;
            if (gfx.activeSelf != box.GetBoxActive())
            {
                gfx.SetActive(box.GetBoxActive());
                boxGo.GetComponent<BoxCollider>().isTrigger = !box.GetBoxActive();
            }
            if (gfx.activeSelf)
            {
                if (gfx.GetComponent<Renderer>().material.color != box.GetBoxColor())
                {
                    gfx.GetComponent<Renderer>().material.color = box.GetBoxColor();
                }
                continue;
            }
            GameObject powerUpGFX = boxGo.transform.Find("PowerUP GFX").gameObject;
            if (powerUpGFX.activeSelf != box.GetPowerUpActive())
            {
                powerUpGFX.SetActive(box.GetPowerUpActive());
            }
            if (powerUpGFX.activeSelf && powerUpGFX.GetComponent<Renderer>().material.color != box.GetPowerUpColor())
            {
                powerUpGFX.GetComponent<Renderer>().material.color = box.GetPowerUpColor();
            }
        }
    }
    public Box[,] GenerMap(int size, RoomInfoClass _setting)
    {
        size = (size % 2 == 0) ? size + 1 : size;
        Box[,] _map = new Box[size, size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool _unbreak = ((x % 2 == 0) && (y % 2 == 0)) || (x == 0) || (x == size - 1) || (y == 0) || (y == size - 1);
                //client.MakeBlock(_unbreak, Random.Range(0, 5) != 0, );
                _map[x, y] = new Box();
                _map[x, y].pos = new Vector2Int(x, y);
                _map[x, y].state = _unbreak ? BlockState.unbrekable : Random.value > _setting.boxDensity ? BlockState.destroyer : BlockState.brekable;
                if (_map[x, y].state != BlockState.destroyer && (Random.value < _setting.powerDensity))
                {
                    if (Random.value > _setting.mysteryPowerDensity)
                    {
                        _map[x, y].PowerUp = (PowerUps)Random.Range(1, 4);
                    }
                    else
                    {
                        _map[x, y].PowerUp = PowerUps.mistery;
                    }
                }
            }
        }
        map = _map;
        return _map;
    }
    

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        foreach(KeyValuePair<int,Client> _cl in RoomManger.allPlayer)
        {
            if (_cl.Value.alive)
            {
                Vector2Int posFloor = new Vector2Int(Mathf.FloorToInt(_cl.Value.GetPly().transform.position.x), Mathf.FloorToInt(_cl.Value.GetPly().transform.position.z));
                if (map[posFloor.x, posFloor.y].PowerUp != PowerUps.none)
                {
                    RoomManger.RoomManagerCom.TakePowerUp(_cl.Value, map[posFloor.x, posFloor.y].PowerUp);
                    map[posFloor.x, posFloor.y].PowerUp = PowerUps.none;
                    RoomManger.RoomManagerCom.StreamSendData(StreamDataType.Map);
                }
            }
        }
    }
    public void SetPowerUp(int x, int y, PowerUps powerUp)
    {
        map[x, y].PowerUp = powerUp;
    }

    public void SetStatus(int x, int y, BlockState state)
    {
        map[x, y].state = state;
    }
    public PowerUps GetPowerUp(int x, int y)
    {
        return map[x, y].PowerUp;
    }
    public BlockState GetStatus(int x, int y)
    {
        return map[x, y].state;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting && PhotonNetwork.IsMasterClient)
        {
            if (Maps == null)
            {
                return;
            }
            List<string> _mapJson = new List<string>();
            foreach (Box _box in boxSync)
            {
                _mapJson.Add(JsonConvert.SerializeObject(_box));
            }
            stream.SendNext(ObjectSerialize.Serialize(JsonConvert.SerializeObject(_mapJson)));
            stream.SendNext(map.GetLength(0));
            stream.SendNext(map.GetLength(1));
        }
        if (stream.IsReading)
        {
            List<string> _mapJson = JsonConvert.DeserializeObject<List<string>>((string)ObjectSerialize.DeSerialize((byte[])stream.ReceiveNext()));
            if (map == null)
            {
                map = new Box[(int)stream.ReceiveNext(), (int)stream.ReceiveNext()];
            }
            List<Box> _boxSync = new List<Box>();
            foreach(string boxJson in _mapJson)
            {
                Box _box = JsonConvert.DeserializeObject<Box>(boxJson);
                _boxSync.Add(_box);
                map[_box.pos.x, _box.pos.y] = _box;
            }
            UpdateMap(_boxSync);
        }
    }

}

public class Box
{
    public BlockState state;
    public PowerUps PowerUp = 0;
    public Vector2Int pos;
    public Color GetBoxColor()
    {
        return state == BlockState.unbrekable ? Color.black : new Color(0.5f, 0.3f, 0f, 1f);
    }
    public bool GetBoxActive()
    {
        return state != BlockState.destroyer;
    }
    public Color GetPowerUpColor()
    {
        Color color = new Color();
        switch (PowerUp)
        {
            case PowerUps.speed:
                color = Color.cyan;
                break;
            case PowerUps.moreBombe:
                color = Color.gray;
                break;
            case PowerUps.moreRiadusse:
                color = Color.red;
                break;
            case PowerUps.mistery:
                color = Color.green;
                break;

        }
        return color;
    }
    public bool GetPowerUpActive()
    {
        return PowerUp != 0 && state == BlockState.destroyer;
    }
}