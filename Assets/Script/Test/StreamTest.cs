using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class StreamTest : MonoBehaviour, IPunObservable
{
    [SerializeField]
    private byte rand;
    [SerializeField]
    private byte echo;

    public void Update()
    {
        rand = (byte)Random.Range(0, 100);
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        
        if (stream.IsWriting )
        {
            stream.SendNext(rand);
        }
        else
        {
            echo = (byte)stream.ReceiveNext();
        }
        
    }
}
