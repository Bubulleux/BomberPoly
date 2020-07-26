using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ErrorBox : MonoBehaviour
{
    public GameObject errorBox;
    public Text msg;
    public void Error(string _msg)
    {
        errorBox.SetActive(true);
        msg.text = _msg;
    }
}
