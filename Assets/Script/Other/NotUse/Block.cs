using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class Block : MonoBehaviour
{

    //public GameObject gfx;
    //public GameObject powerUpGFX;

    //public Material unBreakabelWall;
    //public Material breakabelWall;

    //public Color[] ColorPower;

    //public PhotonView Pv;
    //public List<GameObject> allPlayerInMeTrigger;
    ////private BlockClass classe;
    //public AudioSource audioS;

    //private void Update()
    //{
    //    ////classe = transform.parent.GetComponent<Map>()[BM.Vec3To2(transform.position)];
    //    //allPlayerInMeTrigger = new List<GameObject>();
    //    //gfx.SetActive(classe.state != BlockState.destroyer);
    //    //GetComponent<BoxCollider>().isTrigger = classe.state == BlockState.destroyer;
    //    //gfx.GetComponent<Renderer>().material = classe.state == BlockState.unbrekable ? unBreakabelWall : breakabelWall;

    //    //powerUpGFX.SetActive(classe.PowerUp != 0 && classe.state == BlockState.destroyer);
    //    ////GetComponent<AudioSource>().enabled = classe.PowerUp != 0 && classe.state == BlockState.destroyer;
    //    //if (powerUpGFX.activeSelf)
    //    //{
    //    //    if (classe.PowerUp == PowerUps.mistery)
    //    //    {
    //    //        powerUpGFX.GetComponent<Renderer>().material.color = Color.HSVToRGB(Time.time / 5f % 1f, 1f, 1f);
    //    //    }
    //    //    else
    //    //    {
    //    //        powerUpGFX.GetComponent<Renderer>().material.color = ColorPower[(int)classe.PowerUp - 1];
    //    //    }

    //    //}
    //}
    //public void OnTriggerStay(Collider other)
    //{
    //    allPlayerInMeTrigger.Add(other.gameObject);
        
    //}
    //public void OnTriggerEnter(Collider other)
    //{
        
    //    if  (classe.PowerUp != 0)
    //    {
    //        GetComponent<AudioSource>().Play();
    //        Debug.Log("PlaySound");
    //        if (PhotonNetwork.IsMasterClient)
    //        {
    //            ClientManager.client.Pv.RPC("TakePowerUp", RpcTarget.MasterClient, other.gameObject.GetPhotonView().Owner.ActorNumber, (int)classe.PowerUp);
    //            ClientManager.client.Pv.RPC("DestroyPower", RpcTarget.MasterClient, BM.Vec3To2int(transform.position).x, BM.Vec3To2int(transform.position).y);
    //        }
    //    }
        
    //}
}
public enum BlockState
{
    unbrekable,
    brekable,
    destroyer
}


public enum PowerUps
{
    none,
    speed,
    moreBombe,
    moreRiadusse,
    mistery
}