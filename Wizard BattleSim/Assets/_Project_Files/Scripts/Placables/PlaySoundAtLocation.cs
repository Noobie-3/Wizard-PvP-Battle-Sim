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
        audioSource.clip = sound;
        var Cliplenght = audioSource.clip.length;
        audioSource.Play();
        Destroy(gameObject, Cliplenght);
    }

}
