using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class DataManager : MonoBehaviour
{
    public Datas data = new Datas();
    public void Awake()
    {
        Debug.Log("Start");
        FileStream stream = new FileStream(Application.persistentDataPath + "/data.bin", FileMode.Open);
        BinaryFormatter formatter = new BinaryFormatter();
        string jsonData = (string)formatter.Deserialize(stream);
        stream.Close();
        data = JsonUtility.FromJson<Datas>(jsonData);
        Debug.Log(jsonData);
    }
    

    // Update is called once per frame
    void Update()
    {

    }
    public void Save()
    {
        FileStream stream = new FileStream(Application.persistentDataPath + "/data.bin", FileMode.OpenOrCreate);
        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(stream, JsonUtility.ToJson(data));
        stream.Close();
    }
}

public class Datas
{
    public string name;
    public int kill;
    public int roundWin;
}