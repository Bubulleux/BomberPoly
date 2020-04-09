using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class Block : MonoBehaviour
{

    public GameObject gfx;
    public GameObject powerUpGFX;

    public Material unBreakabelWall;
    public Material breakabelWall;

    public Color[] ColorPower;

    public PhotonView Pv;
    public List<GameObject> allPlayerInMeTrigger;
    private BlockClass classe;

    private void Update()
    {
        classe = ClientManager.client.Blocks[BM.Vec3To2(transform.position)];
        allPlayerInMeTrigger = new List<GameObject>();
        gfx.SetActive(classe.state != BlockState.destroyer);
        GetComponent<BoxCollider>().isTrigger = classe.state == BlockState.destroyer;
        gfx.GetComponent<Renderer>().material = classe.state == BlockState.unbrekable ? unBreakabelWall : breakabelWall;

        powerUpGFX.SetActive(classe.PowerUp != 0 && classe.state == BlockState.destroyer);
        if (powerUpGFX.activeSelf)
        {
            powerUpGFX.GetComponent<Renderer>().material.color = ColorPower[classe.PowerUp - 1];

        }
    }
    public void OnTriggerStay(Collider other)
    {
        allPlayerInMeTrigger.Add(other.gameObject);
        
    }
    public void OnTriggerEnter(Collider other)
    {
        
        if  (classe.PowerUp != 0)
        {
            GetComponent<AudioSource>().Play();
            if (PhotonNetwork.IsMasterClient)
            {
                ClientManager.client.Pv.RPC("TakePowerUp", RpcTarget.MasterClient, other.gameObject.GetPhotonView().Owner.ActorNumber, classe.PowerUp);
                ClientManager.client.Pv.RPC("DestroyPower", RpcTarget.MasterClient, BM.Vec3To2int(transform.position).x, BM.Vec3To2int(transform.position).y);
            }
        }
    }
}
public enum BlockState
{
    unbrekable,
    brekable,
    destroyer
}