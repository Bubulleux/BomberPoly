using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Spectator : MonoBehaviour
{
    [SerializeField]
    private int index;
    public GameObject cam;
    [SerializeField]
    private bool freeCam = false;
    private int y = 15;
    private void Start()
    {
        index = 0;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            freeCam = !freeCam;
        }
        if (freeCam)
        {
            transform.parent = null;
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                index++;
            }
            if (Input.GetMouseButtonDown(1))
            {
                index--;
            }
            if (index<0)
            {
                index = ClientManager.client.playerCount -1 ;
            }
            try
            {
                transform.position = ply(index).transform.position;
            }
            catch
            {
                
                int _i = 0;
                while (ply(index) == null)
                {
                    Debug.Log("While "+ ply(index));
                    _i++;
                    index++;
                    index = index % ClientManager.client.playerCount;
                    if (_i >= ClientManager.client.playerCount)
                    {
                        
                        freeCam = true;
                        break;
                    }
                }
                Debug.Log(ply(index));
            }
            
        }
        y += Mathf.FloorToInt(Input.GetAxis("Mouse ScrollWheel"));
        y = y > 30 ? 30 : y;
        y = y < 10 ? 10 : y;
        transform.position = new Vector3(transform.position.x, y, transform.position.z);
    }
    private void FixedUpdate()
    {
        if (freeCam)
        {
            Vector3 _force = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")).normalized * 10f;
            GetComponent<Rigidbody>().AddForce(_force);
        }
        
    }
    private void ChangeCam(int _index)
    {
        
    }
    private GameObject ply(int _index)
    {
        int _i = 0;
        foreach(KeyValuePair<int, PlayerData> _v in ClientManager.client.allPlayer)
        {
            if (_i == _index)
            {
                if (_v.Value.alive)
                {
                    Debug.Log("Ply Find " + _index);
                    return PhotonView.Find(_v.Value.palyerGOId).gameObject;
                }
                else
                {
                    Debug.Log("Ply not " + _index);
                    return null;
                }
            }
            _i++;
        }
        Debug.Log("Ply not " + _index);
        return null;
    }
}
