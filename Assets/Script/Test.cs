using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class Test : MonoBehaviour
{
       
    void Start()
    {
        string s = JsonUtility.ToJson(new test());
        //Debug.Log(s);
        
    }
    public class test
    {
        public float[] list = new float[]
        {
            0.5f,
            13f,
            43.32f,
            93.43f
        };
    }
}
