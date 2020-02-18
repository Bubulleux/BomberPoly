using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class StreamTest : MonoBehaviour, IPunObservable
{
    [SerializeField]
    private int rand;
    [SerializeField]
    private int echo;

    public void Update()
    {
        rand = Random.Range(0, 100);
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        
        if (stream.IsWriting )
        {
            stream.SendNext(rand);
        }
        else
        {
            echo = (int)stream.ReceiveNext();
        }
        
    }
}
