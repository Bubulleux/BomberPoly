using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class StreamManager : MonoBehaviour, IPunObservable
{
    public  Dictionary<int, PlayerData> allPlayer = new Dictionary<int, PlayerData>();
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting && PhotonNetwork.IsMasterClient)
        {
            byte[] bytesSteamed;
            //Players
            Dictionary<int, string> allPlayerJSON = new Dictionary<int, string>();
            foreach(KeyValuePair<int, PlayerData> value in allPlayer)
            {
                allPlayerJSON[value.Key] = JsonUtility.ToJson(value.Value);
            }
            bytesSteamed = ObjectSerialize.Serialize(allPlayerJSON);
            stream.SendNext(bytesSteamed);

            //BLocks
            Dictionary<string, string> BlocksJSON = new Dictionary<string, string>();
            ClientManager.client.Blocks = RoomManger.RoomManagerCom.Blocks;
            foreach (KeyValuePair<Vector2, BlockClass> value in RoomManger.RoomManagerCom.Blocks)
            {
                BlocksJSON[JsonUtility.ToJson(value.Key)] = JsonUtility.ToJson(value.Value);
                if (Input.GetKey(KeyCode.F6))
                {
                    Debug.Log(string.Format("key: {0}, value: {1}, key Not JSON: {2}, count: {3}", JsonUtility.ToJson(value.Key), JsonUtility.ToJson(value.Value), value.Key, BlocksJSON.Count));
                }
            }
            //if (Input.GetKey(KeyCode.F6))
            //{
            //    Debug.Log(RoomManger.RoomManagerCom.Blocks.Count+"  "+BlocksJSON.Count);
            //}
            bytesSteamed = ObjectSerialize.Serialize(BlocksJSON);
            stream.SendNext(bytesSteamed);

            stream.SendNext(RoomManger.RoomManagerCom.roundStat);
            ClientManager.client.roundStat = RoomManger.RoomManagerCom.roundStat;

        }
        else if (stream.IsReading)
        {
            byte[] bytereceive;
            bytereceive = (byte[])stream.ReceiveNext();
            Dictionary<int, string> allPlayerJSON = (Dictionary<int, string>) ObjectSerialize.DeSerialize(bytereceive);
            allPlayer.Clear();
            foreach(KeyValuePair<int, string> v in allPlayerJSON)
            {
                allPlayer[v.Key] = JsonUtility.FromJson<PlayerData>(v.Value); 
            }

            bytereceive = (byte[])stream.ReceiveNext();
            Dictionary<string, string> BlocksJSON = (Dictionary<string, string>)ObjectSerialize.DeSerialize(bytereceive);
            ClientManager.client.Blocks.Clear();
            
            foreach (KeyValuePair<string, string> v in BlocksJSON)
            {
                ClientManager.client.Blocks[JsonUtility.FromJson<Vector2>(v.Key)] = JsonUtility.FromJson<BlockClass>(v.Value);

            }
            if (Input.GetKey(KeyCode.F6))
            {
                Debug.Log(ClientManager.client.Blocks.Count);
            }

            ClientManager.client.roundStat = (roundInfo) stream.ReceiveNext();
            
        }
    }
}
