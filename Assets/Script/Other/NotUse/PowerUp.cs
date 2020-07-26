using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PowerUp : MonoBehaviour
{
    public Material[] Materials;
    public GameObject gfx;
    public int powerUp;
    public void Initilase(int _PowerUpId)
    {
        powerUp = _PowerUpId;
        gfx.GetComponent<MeshRenderer>().material = Materials[_PowerUpId - 1];
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            //ClientManager.client.TakePowerUp1(other.GetComponent<Player>().viewIDclient, powerUp);
            ClientManager.client.DestroyGameObj(gameObject);
        }
    }
}
