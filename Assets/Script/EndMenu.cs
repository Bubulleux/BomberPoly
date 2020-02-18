using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndMenu : MonoBehaviour
{
    public string winer;
    public Text winnerText;
    public GameObject PrefabScorPl;
    public Transform ScorBoard;

    public void EndGame()
    {
        winnerText.text = winer + " won";
        foreach(GameObject _go in GameObject.FindGameObjectsWithTag("PlyScore"))
        {
            Destroy(_go);
        }
        foreach (KeyValuePair<int, PlayerData> _pl in ClientManager.client.allPlayer)
        {
            GameObject _Obj = Instantiate(PrefabScorPl, ScorBoard);
            _Obj.transform.Find("Name").GetComponent<Text>().text = _pl.Value.name;
            _Obj.transform.Find("Name").GetComponent<Text>().color = _pl.Value.color;
            _Obj.transform.Find("Kill").GetComponent<Text>().text = "K :" + _pl.Value.kill.ToString();
            _Obj.transform.Find("Win").GetComponent<Text>().text = "W :" + _pl.Value.win.ToString();
        }
    }
}
