using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Music : MonoBehaviour
{
    public AudioMixer audioMixer;
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        if (FindObjectsOfType<Music>().Length != 1)
        {
            Destroy(gameObject);
        }
    }
    void Update()
    {
        audioMixer.SetFloat("MainVolume", DataManager.GetData().mainVolume);
        audioMixer.SetFloat("MusicVolume", DataManager.GetData().musicVolume);
    }
}
