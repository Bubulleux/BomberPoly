using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class DataManager 
{
    private static Datas constDatas;
    public static void Save(Datas data)
    {
        FileStream stream = new FileStream(Application.persistentDataPath + "/data.bin", FileMode.OpenOrCreate);
        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(stream, JsonUtility.ToJson(data));
        stream.Close();
        VerifQuality();
        SetData();
    }
    public static Datas GetData()
    {
        if (constDatas == null)
        {
            SetData();
        }
        return constDatas;
    }
    private static void SetData()
    {
        try
        {
            FileStream stream = new FileStream(Application.persistentDataPath + "/data.bin", FileMode.Open);
            BinaryFormatter formatter = new BinaryFormatter();
            string jsonData = (string)formatter.Deserialize(stream);
            stream.Close();
            constDatas = JsonUtility.FromJson<Datas>(jsonData);
        }
        catch
        {
            Debug.Log("Data Not found");
            constDatas = new Datas();
            Save(constDatas);
        }
        VerifQuality();
    }
    private static void VerifQuality()
    {
        if (QualitySettings.GetQualityLevel() != GetData().quality)
        {
            QualitySettings.SetQualityLevel(GetData().quality);
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
    public float mainVolume = 0f;
    public float musicVolume = 0f;
}