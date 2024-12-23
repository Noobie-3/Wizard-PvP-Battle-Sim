using UnityEngine;
using UnityEngine.Rendering;

public class MusicManager : MonoBehaviour
{
    public GameObject MusicPlayer;
    public AudioClip MusicClip;
    
    public void PlaySong()
    {
        MusicPlayer = gameController.GC.PlaySoundAtLocation(transform, MusicClip);
        MusicPlayer.GetComponent<PlaySoundAtLocation>().DoDestory = false;
        MusicPlayer.GetComponent<AudioSource>().spatialBlend = 0;

    }
}
