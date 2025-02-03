using Unity.Netcode;
using UnityEngine;

public class PlayerSceneNotifier : NetworkBehaviour
{
    public ulong CLientId;
    public MusicManager musicManager;
    public AudioClip battleMusic;
    public void Start()
    {
        musicManager = FindObjectOfType<MusicManager>();
    }

    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.SceneManager.OnSceneEvent += NotifySceneLoad;
    }
    public void NotifySceneLoad(SceneEvent Sevent)
    {
        if(Sevent.SceneEventType == SceneEventType.LoadEventCompleted )
        {
            musicManager.PlaySong(battleMusic);
            if (IsHost) return;
            print("Scene Loaded by client " + CLientId);
            SpawnManager.instance.SpawnPlayerServerRpc(CLientId);
        }
    }

}
