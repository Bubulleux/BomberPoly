using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Newtonsoft.Json;

public class StreamManager : MonoBehaviour, IPunObservable
{
    public  Dictionary<int, PlayerData> allPlayer = new Dictionary<int, PlayerData>();
    private bool[] whyUpdate = new bool[2];
    /*
     * 0: Players
     * 1: Block
     */
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting && PhotonNetwork.IsMasterClient)
        {
            stream.SendNext(ObjectSerialize.Serialize(whyUpdate));
            stream.SendNext(Random.Range(100, 10000).ToString());
            Debug.Log("IsWriting");
            byte[] bytesSteamed;
            //Players
            if (whyUpdate[0])
            {
                //Dictionary<int, string> allPlayerJSON = new Dictionary<int, string>();
                //foreach (KeyValuePair<int, PlayerData> value in allPlayer)
                //{
                //    allPlayerJSON[value.Key] = JsonUtility.ToJson(value.Value);
                //}
                //bytesSteamed = ObjectSerialize.Serialize(allPlayerJSON);
                //stream.SendNext(bytesSteamed);
                string dataJson = JsonConvert.SerializeObject(allPlayer);
                bytesSteamed = ObjectSerialize.Serialize(dataJson);
                Debug.Log("byte streamed:" + bytesSteamed.Length);
                stream.SendNext(bytesSteamed);
            }
            

            //BLocks
            if (whyUpdate[1])
            {
                Dictionary<string, string> BlocksJSON = new Dictionary<string, string>();
                ClientManager.client.Blocks = RoomManger.RoomManagerCom.blocks;
                foreach (KeyValuePair<Vector2, BlockClass> value in RoomManger.RoomManagerCom.blocks)
                {
                    BlocksJSON[JsonUtility.ToJson(value.Key)] = JsonUtility.ToJson(value.Value);
                    if (Input.GetKey(KeyCode.F6))
                    {
                        Debug.Log(string.Format("key: {0}, value: {1}, key Not JSON: {2}, count: {3}", JsonUtility.ToJson(value.Key), JsonUtility.ToJson(value.Value), value.Key, BlocksJSON.Count));
                    }
                }
                bytesSteamed = ObjectSerialize.Serialize(BlocksJSON);
                stream.SendNext(bytesSteamed);
            }
            

            stream.SendNext(RoomManger.RoomManagerCom.roominfo.roundInfo);
            ClientManager.client.roundStat = RoomManger.RoomManagerCom.roominfo.roundInfo;
            whyUpdate = new bool[2];
        }
        else if (stream.IsReading)
        {
            Debug.Log("Is Reading");
            bool[] _whyUpdate = (bool[]) ObjectSerialize.DeSerialize((byte[])stream.ReceiveNext());
            Debug.Log((string)stream.ReceiveNext());
            byte[] bytereceive;
            //Player
            if (_whyUpdate[0])
            {
                //bytereceive = (byte[])stream.ReceiveNext();
                //Dictionary<int, string> allPlayerJSON = (Dictionary<int, string>)ObjectSerialize.DeSerialize(bytereceive);
                //allPlayer.Clear();
                //foreach (KeyValuePair<int, string> v in allPlayerJSON)
                //{
                //    allPlayer[v.Key] = JsonUtility.FromJson<PlayerData>(v.Value);
                //}
                bytereceive = (byte[])stream.ReceiveNext();
                Debug.Log("byte recevive:" + bytereceive.Length);
                string DataJson = (string)ObjectSerialize.DeSerialize(bytereceive);
                allPlayer = (Dictionary<int, PlayerData>)JsonConvert.DeserializeObject(DataJson, typeof(Dictionary<int, PlayerData>));
                Debug.Log(DataJson);
            }
            

            //Block
            if (_whyUpdate[1])
            {
                bytereceive = (byte[])stream.ReceiveNext();
                Dictionary<string, string> BlocksJSON = (Dictionary<string, string>)ObjectSerialize.DeSerialize(bytereceive);
                ClientManager.client.Blocks.Clear();

                foreach (KeyValuePair<string, string> v in BlocksJSON)
                {
                    ClientManager.client.Blocks[JsonUtility.FromJson<Vector2>(v.Key)] = JsonUtility.FromJson<BlockClass>(v.Value);

                }
            }
            

            ClientManager.client.roundStat = (roundInfo) stream.ReceiveNext();
        }
    }

    public void WhyUpdate(int _i)
    {
        whyUpdate[_i] = true;
    }
}
