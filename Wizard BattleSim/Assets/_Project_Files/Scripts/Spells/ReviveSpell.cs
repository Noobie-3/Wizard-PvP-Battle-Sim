using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ReviveSpell : NetworkBehaviour , ISpell_Interface

{
    public ulong CasterId { get; set; }
    public Vector3 Direction { get; set; }
    public Vector3 RevivePoint;
    public Spell Spell;
    Spell ISpell_Interface.spell => Spell;
    public float hitagainTime { get; set; }
    public float CurrentLifeTime;
    public PlayerController playerController;
    public bool CanRevive = false;
    public GameObject ReviveEffect_Partical;
    public void FireSpell()
    {

    }

    public void Initialize(ulong casterId, Vector3 direction)
    {
        Direction = direction;
        CasterId = casterId;
        CanRevive = true;
        //Find Player and set CanDie to false
        foreach (var player in FindObjectsOfType<PlayerController>())
        {
            if (player.OwnerClientId == CasterId)
            {
                player.CanDie = false;
                playerController = player;
            }
        }
    }

    public void TimeOutSpell()  
    {
        if(!IsOwner) return;

        if (!CanRevive) return;
        if(CurrentLifeTime >= Spell.LifeTime)
        {
            //Died Out effect


            Destroy(gameObject);
        }
        else
        {
            CurrentLifeTime += Time.deltaTime;
        }

    }
    private void FixedUpdate()
    {
        if(!IsOwner) return;
        //move spell torwards revive point
        if (Vector3.Distance(transform.position, RevivePoint) <= 1)
        {
            CanRevive = true;

        }
        else
        {
            transform.Translate(Direction * Spell.Spell_Speed * Time.deltaTime);
        }

        if (!CanRevive) return;
        TimeOutSpell();



        if (playerController.Stats.Health.Value <= 0)
        {
            playerController.Stats.Health.Value = playerController.Stats.MaxHealth.Value;


            //Play some Sound effects and partcicals
            if (IsServer)
            {
                if(ReviveEffect_Partical != null)
                {
                    Instantiate(ReviveEffect_Partical, playerController.transform.position, default);

                }
            }
            Destroy(gameObject);
        }

    }

    public void TriggerEffect()
    {
        throw new System.NotImplementedException();
    }
}
