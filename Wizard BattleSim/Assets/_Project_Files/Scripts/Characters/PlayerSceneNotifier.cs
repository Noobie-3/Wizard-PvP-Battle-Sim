using Unity.Netcode;
using UnityEngine;

public class PlayerSceneNotifier : NetworkBehaviour
{
    public ulong CLientId;

    public void Start()
    {

    }

    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.SceneManager.OnSceneEvent += NotifySceneLoad;
    }
    public void NotifySceneLoad(SceneEvent Sevent)
    {
        if(Sevent.SceneEventType == SceneEventType.LoadEventCompleted )
        {
            if(IsServer && IsOwner)
            {
                SpawnManager.instance.AssignSpawnPointsByServer();
            } 
            print("Scene Loaded by client " + CLientId);
            SpawnManager.instance.SpawnPlayerServerRpc(CLientId);
        }
    }

}
