using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingSender : MonoBehaviour
{
    public GameObject settingPanel;
    public GameObject text;
    public Text nameText;
    private string name;
    private SettingType type;
    void Start()
    {
        settingPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void GetSetting(string _name, SettingType _type)
    {
        name = _name;
        type = _type;
        nameText.text = _name;
        if (type == SettingType.String)
        {
            text.SetActive(true);
        }
    }
    public void Done()
    {
        settingPanel.SetActive(false);
    }
}
public enum SettingType
{
    Color,
    String
}
