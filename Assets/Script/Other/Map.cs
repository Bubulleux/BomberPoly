using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Map : MonoBehaviour
{
    public BlockClass[,] map;
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
        for (int i = 0; i < transform.childCount; i++)
        {
            
            GameObject _boxGo = transform.GetChild(i).gameObject;
            BlockClass _box = map[Mathf.FloorToInt(_boxGo.transform.position.x), Mathf.FloorToInt(_boxGo.transform.position.z)];
            GameObject gfx = _boxGo.transform.Find("GFX").gameObject;
            GameObject powerUpGFX = _boxGo.transform.Find("PowerUP GFX").gameObject;
            gfx.SetActive(_box.state != BlockState.destroyer);
            _boxGo.GetComponent<BoxCollider>().isTrigger = _box.state == BlockState.destroyer;
            gfx.GetComponent<Renderer>().material.color = _box.state == BlockState.unbrekable ? Color.black : new Color(0.5f, 0.3f, 0f, 1f);

            powerUpGFX.SetActive(_box.PowerUp != 0 && _box.state == BlockState.destroyer);
            //GetComponent<AudioSource>().enabled = _box.PowerUp != 0 && _box.state == BlockState.destroyer;
            if (powerUpGFX.activeSelf)
            {
                Color color = new Color();
                switch (_box.PowerUp)
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
                powerUpGFX.GetComponent<Renderer>().material.color = color;

            }
        }
    }
    public static BlockClass[,] GenerMap(int size, RoomInfoClass _setting)
    {
        size = (size % 2 == 0) ? size + 1 : size;
        BlockClass[,] _map = new BlockClass[size, size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool _unbreak = ((x % 2 == 0) && (y % 2 == 0)) || (x == 0) || (x == size - 1) || (y == 0) || (y == size - 1);
                //client.MakeBlock(_unbreak, Random.Range(0, 5) != 0, );
                _map[x, y] = new BlockClass();
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
                    RoomManger.RoomManagerCom.map[posFloor.x, posFloor.y].PowerUp = PowerUps.none;
                    RoomManger.RoomManagerCom.StreamSendData(StreamDataType.Map);
                }
            }
        }
    }
}
