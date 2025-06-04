using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

[RequireComponent(typeof(NetworkObject))]
public class Network_Animator_Client : NetworkBehaviour
{
    public Animator animator;

    [Header("Parameter Sync Settings")]
    public bool syncBools = true;
    public bool syncInts = true;
    public bool syncFloats = true;

    private Dictionary<int, AnimatorControllerParameterType> paramTypes = new();

    void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        foreach (var param in animator.parameters)
            paramTypes[param.nameHash] = param.type;
    }

    void Update()
    {
        if (!IsOwner) return;

        foreach (var param in animator.parameters)
        {
            int hash = param.nameHash;
            switch (param.type)
            {
                case AnimatorControllerParameterType.Bool:
                    if (syncBools)
                        SendBoolServerRpc(hash, animator.GetBool(hash));
                    break;
                case AnimatorControllerParameterType.Int:
                    if (syncInts)
                        SendIntServerRpc(hash, animator.GetInteger(hash));
                    break;
                case AnimatorControllerParameterType.Float:
                    if (syncFloats)
                        SendFloatServerRpc(hash, animator.GetFloat(hash));
                    break;
            }
        }
    }

    [ServerRpc]
    void SendBoolServerRpc(int hash, bool value)
    {
        animator.SetBool(hash, value);
        SendBoolClientRpc(hash, value);
    }

    [ClientRpc]
    void SendBoolClientRpc(int hash, bool value)
    {
        if (IsOwner) return;
        animator.SetBool(hash, value);
    }

    [ServerRpc]
    void SendIntServerRpc(int hash, int value)
    {
        animator.SetInteger(hash, value);
        SendIntClientRpc(hash, value);
    }

    [ClientRpc]
    void SendIntClientRpc(int hash, int value)
    {
        if (IsOwner) return;
        animator.SetInteger(hash, value);
    }

    [ServerRpc]
    void SendFloatServerRpc(int hash, float value)
    {
        animator.SetFloat(hash, value);
        SendFloatClientRpc(hash, value);
    }

    [ClientRpc]
    void SendFloatClientRpc(int hash, float value)
    {
        if (IsOwner) return;
        animator.SetFloat(hash, value);
    }
}
