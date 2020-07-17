using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class Bombe : MonoBehaviour
{
    private bool start = true;
    public void Start()
    {
        StartCoroutine(DisableStart());
        foreach(GameObject _go in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (_go.GetPhotonView().Owner != PhotonNetwork.LocalPlayer)
            {
                Physics.IgnoreCollision(_go.GetComponent<Collider>(), GetComponent<Collider>());
            }

        }
    }
    IEnumerator DisableStart()
    {
        yield return new WaitForSeconds(0.5f);
        start = false;
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
