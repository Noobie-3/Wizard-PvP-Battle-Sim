using UnityEngine;
using UnityEngine.Rendering;

public class MusicManager : MonoBehaviour
{
    public GameObject MusicPlayer;
    public AudioClip MusicClip;
    

    public void Start()
    {
        DontDestroyOnLoad(gameObject);
        PlaySong(MusicClip);
    }
    public void PlaySong(AudioClip music)
    {
        if(MusicPlayer == null)
        {
            MusicPlayer = gameController.GC.PlaySoundAtLocation(transform, music, .25f, 160, DoDestroy:false);
            MusicPlayer.GetComponent<AudioSource>().spatialBlend = 0;
            DontDestroyOnLoad(MusicPlayer);
        }
        else
        {
            MusicPlayer.GetComponent<AudioSource>().clip = music;
            MusicPlayer.GetComponent<AudioSource>().Play();
        }

    }
}
