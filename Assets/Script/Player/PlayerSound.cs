using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Audio;

public class PlayerSound : MonoBehaviour
{
    public AudioMixerGroup mixer;
    public AudioClip powerUpClip;
    [PunRPC]
    void PowerUpSound(int x, int y)
    {
        Music.playSound(powerUpClip, mixer, new Vector3(x + 0.5f, 0.5f, y + 0.5f));
    }
}
