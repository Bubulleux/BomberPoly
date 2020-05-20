using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
public class ButonManager : MonoBehaviour
{
    public PunManager pun;
    public Button playBut;
    public GameObject mainMenu;
    public GameObject setting;
    private DataManager dataManager;

    public InputField nameSetting;
    public Dropdown hatSetting;
    void Start()
    {
        pun = GetComponent<PunManager>();
        dataManager = GetComponent<DataManager>();
        hatSetting.AddOptions(PlayerGo.hats);
        //foreach(string _hat in PlayerGo.hats)
        //{
        //    hatSetting.options.Add(new Dropdown.OptionData(text: _hat));
        //}
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
            nameSetting.text = dataManager.data.name;
            hatSetting.value = dataManager.data.hat;
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
        dataManager.data.name = nameSetting.text;
        dataManager.data.hat = hatSetting.value;
        dataManager.Save();
        mainMenu.SetActive(true);
        setting.SetActive(false);
    }
    //public void OnclickGetSetting(string _name)
    //{
    //    if (_name == "name")
    //    {
    //        setttingFunc.GetSetting(_name, SettingType.String);
    //    }
        
    //}
}
