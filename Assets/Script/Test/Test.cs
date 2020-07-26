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
        Box box = new Box();
        box.pos = new Vector2Int(100, 200);
        Debug.Log(JsonConvert.SerializeObject(box));
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
