using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using Newtonsoft.Json;

public class Test : MonoBehaviour
{

    void Start()
    {
        PlayerVar ply = new PlayerVar();
        ply.name = "Bub";
        ply.kill = 12;
        ply.powerUps = new Dictionary<PowerUps, object>();
        ply.powerUps.Add(PowerUps.moreBombe, 3);
        ply.color = new Color32(0,0,0,0);
    }
}

