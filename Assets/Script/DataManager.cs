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
        FileStream stream = new FileStream(Application.persistentDataPath + "/data.bin", FileMode.Open);
        BinaryFormatter formatter = new BinaryFormatter();
        string jsonData = (string)formatter.Deserialize(stream);
        stream.Close();
        data = JsonUtility.FromJson<Datas>(jsonData);
        VerifQuality();
    }
    
    public void Save()
    {
        FileStream stream = new FileStream(Application.persistentDataPath + "/data.bin", FileMode.OpenOrCreate);
        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(stream, JsonUtility.ToJson(data));
        stream.Close();
        VerifQuality();
    }
    public void VerifQuality()
    {
        if (QualitySettings.GetQualityLevel() != data.quality)
        {
            QualitySettings.SetQualityLevel(data.quality);
            Debug.Log("Quality level Set to " + QualitySettings.names[QualitySettings.GetQualityLevel()]);
        }
    }
}

public class Datas
{
    public string name = "No Name";
    public int hat;
    public int quality;
    public int kill;
    public int roundWin;
}