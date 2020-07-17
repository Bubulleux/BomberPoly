using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class Explision : MonoBehaviour
{
    private float size;
    void Update()
    {
        size += Time.deltaTime;
        if (size >= 2f && GetComponent<PhotonView>().IsMine)
        {
            //size = 1f;
            ClientManager.client.DestroyGameObj(gameObject);
        }
    }
}
