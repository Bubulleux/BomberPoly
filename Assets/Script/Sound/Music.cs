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

    public static void playSound(AudioClip _sound, AudioMixerGroup _mixer , Vector3 _pos)
    {
        GameObject _go = new GameObject("Sound");
        _go.transform.position = _pos;
        _go.AddComponent<AudioSource>();
        _go.GetComponent<AudioSource>().clip = _sound;
        _go.GetComponent<AudioSource>().outputAudioMixerGroup = _mixer;
        _go.GetComponent<AudioSource>().Play();
        Destroy(_go, _sound.length);
    }
}
