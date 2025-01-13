using System;
using System.Diagnostics.Contracts;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlaySoundAtLocation : MonoBehaviour
{
    public AudioClip sound;
    public  AudioSource audioSource;
    public bool DoDestory = true;
    void Start()
    {
         audioSource = GetComponent<AudioSource>();
    }

    public void SetvaluesAndPlay(float Volume = .5f, int Priority = 140, int Pitch = 1) {

        audioSource = GetComponent<AudioSource>();
        audioSource.clip = sound;
        audioSource.volume = Volume;
        audioSource.priority = Priority;
        audioSource.pitch = Pitch;
        PlayAndDestroy();
    }

    public void PlayAndDestroy()
    {
        var Cliplenght = audioSource.clip.length;
        audioSource.Play();

        if(DoDestory)
        Destroy(gameObject, Cliplenght);
    }

}
