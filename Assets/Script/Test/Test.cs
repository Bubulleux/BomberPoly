using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using Newtonsoft.Json;
using System.Reflection;
using System;

public class Test : MonoBehaviour
{

    void Start()
    {
        //Debug.Log(JsonConvert.DeserializeObject<Client>(new Client().json()).name);
        Client client = new Client();
        foreach (FieldInfo field in client.GetType().GetFields())
        {
            //Debug.LogFormat("name {0} value {1}", field.Name, field.GetValue(client));
        }
    }
}
public class Foo
{
    public int intVar = 10;
    public string stringVar = "Chaise";
    public GameObject go = null;
    public Foo(GameObject _go)
    {
        go = _go;
    }
}
