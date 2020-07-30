using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Newtonsoft.Json;

public class ServerStreamer : MonoBehaviour, IPunObservable
{
    public PlayerCl plyCl;
    public void OnPhotonSerializeView(PhotonStream _stream, PhotonMessageInfo info)
    {
        if (_stream.IsWriting)
        {
            List<string> _jsonData = new List<string>
                {
                    JsonConvert.SerializeObject(plyCl.mysteryPower),
                    JsonConvert.SerializeObject(plyCl.BombeCount),
                    JsonConvert.SerializeObject(plyCl.powerUps)
                };
            string _json = JsonConvert.SerializeObject(_jsonData);
            _stream.SendNext(ObjectSerialize.Serialize(_json));
        }
        else
        {
            List<string> _jsonData = JsonConvert.DeserializeObject<List<string>>((string)ObjectSerialize.DeSerialize((byte[])_stream.ReceiveNext()));
            plyCl.mysteryPower = JsonConvert.DeserializeObject<MysteryPower.MysteryPowers>(_jsonData[0]);
            plyCl.BombeCount = JsonConvert.DeserializeObject<int>(_jsonData[1]);
            plyCl.powerUps = JsonConvert.DeserializeObject<Dictionary<PowerUps, int>>(_jsonData[2]);
        }
    }
}
