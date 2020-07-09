using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ButonManager : MonoBehaviour
{
    public PunManager pun;
    public Button playBut;
    public GameObject mainMenu;
    public GameObject setting;

    public InputField nameSetting;
    public Dropdown hatSetting;
    public Dropdown qualitySetting;
    public Slider mainVolume;
    public Slider musicVolume;
    void Start()
    {
        pun = GetComponent<PunManager>();
        hatSetting.AddOptions(PlayerCl.hats);
        List<string> _qualitys = new List<string>();
        foreach (string _qualityName in QualitySettings.names)
        {
            _qualitys.Add(_qualityName);
        }
        qualitySetting.AddOptions(_qualitys);
    }
    void Update()
    {
        playBut.interactable = pun.onLooby;
        playBut.GetComponentInChildren<Text>().text = pun.onLooby ? "Play" : "Loading...";
    }
    public void OnClickPlay()
    {
        pun.ConnectToRoom();
    }
    public void OnClickSetting()
    {
        try
        {
            Datas data = DataManager.GetData();
            nameSetting.text = data.name;
            hatSetting.value = data.hat;
            hatSetting.value = data.quality;
            mainVolume.value = data.mainVolume;
            musicVolume.value = data.musicVolume;
        }
        catch
        {
            Debug.LogError("Data Setting Not Found");
        }
        mainMenu.SetActive(false);
        setting.SetActive(true);
    }
    public void OnClickQuit()
    {
        Application.Quit();
        Debug.LogWarning("Quit");
    }
    public void OnClickSave()
    {
        Datas data = new Datas();
        data.name = nameSetting.text;
        data.hat = hatSetting.value;
        data.quality = qualitySetting.value;
        data.mainVolume = mainVolume.value;
        data.musicVolume = musicVolume.value;
        DataManager.Save(data);
        mainMenu.SetActive(true);
        setting.SetActive(false);
    }
}
