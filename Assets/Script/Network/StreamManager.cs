using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Newtonsoft.Json;
using System;
using BayatGames.Serialization.Formatters.Binary;

public class StreamManager : MonoBehaviour, IPunObservable
{
    private Dictionary<StreamDataType, string> streamDatas = new Dictionary<StreamDataType, string>();
    public int byteStreamed;
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting && PhotonNetwork.IsMasterClient)
        {
            if (streamDatas.Count != 0)
            {
                string _dataJson = JsonConvert.SerializeObject(streamDatas);
                byte[] _dataByte = ObjectSerialize.Serialize(_dataJson);
                stream.SendNext(_dataByte.Length);
                stream.SendNext(_dataByte);
                streamDatas.Clear();
                byteStreamed = _dataByte.Length;
            }
            else
            {
                stream.SendNext(0);
            }
        }
        if (stream.IsReading)
        {
            int _byteCount = (int)stream.ReceiveNext();
            if (_byteCount != 0)
            {
                byteStreamed = _byteCount;
                byte[] _dataBytes = (byte[])stream.ReceiveNext();
                string _dataJson = (string)ObjectSerialize.DeSerialize(_dataBytes);
                Dictionary<StreamDataType, string> _datas = (Dictionary<StreamDataType, string>)JsonConvert.DeserializeObject(_dataJson, typeof(Dictionary<StreamDataType, string>));
                foreach(KeyValuePair<StreamDataType, string> _data in _datas)
                {
                    ClientManager.client.RecevingData(_data.Key, _data.Value);
                }

            }
        }
    }
    public void SendData(StreamDataType _type, string _data)
    {
        if (_data == null)
        {
            Debug.LogError("Data Is null");
            return;
        }
        if (!streamDatas.ContainsKey(_type))
        {
            streamDatas.Add(_type, _data);
        }
        else
        {
            streamDatas[_type] = _data;
        }
        ClientManager.client.RecevingData(_type, _data);
    }
} 

public enum StreamDataType
{
    Players,
    Map,
    Room
}