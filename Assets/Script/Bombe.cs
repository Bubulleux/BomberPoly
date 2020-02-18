using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class Bombe : MonoBehaviour
{
    private bool start = true;
    public void Update()
    {
        if (start)
        {
            start = false;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetPhotonView().Owner == PhotonNetwork.LocalPlayer)
        {
            GetComponent<BoxCollider>().isTrigger = false;
        }
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetPhotonView().Owner == PhotonNetwork.LocalPlayer && !start)
        {
            GetComponent<BoxCollider>().isTrigger = false;
        }
    }

}
