using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class MeteorShowerScript : NetworkBehaviour, ISpell_Interface
{

    //LEagacy code
    [SerializeField] private float RadiusOfEffect;
    [SerializeField] private GameObject Meteor_Prefab;
    [SerializeField] public Spell spell;
    public float hitagainTime { get; set; }
    public float CurrentLifeTime;
    [SerializeField] private float MeteorHeight;
    [SerializeField] Coroutine SpawnCoroutine;
    public bool CanHit;

    public ulong CasterId { get; set; }
    
    Spell ISpell_Interface.spell => spell;

    public Rigidbody rb;

    // Update is called once per frame
    void Update()
    {
        CurrentLifeTime += Time.deltaTime;
        if(hitagainTime  >= spell.MultiHitCooldown)
        {
            hitagainTime= 0;
            CanHit = true;
        }
        else
        {
            hitagainTime += Time.deltaTime;
        }
    }

    public IEnumerator SpawnMeteors()
    {
        while (CurrentLifeTime < spell.LifeTime)
        {
            var randomMeteorSize = Random.Range(0.5f, 2);
            var randomMeteorPosition = new Vector3(Random.Range(-RadiusOfEffect, RadiusOfEffect), MeteorHeight, Random.Range(-RadiusOfEffect, RadiusOfEffect));
            var SpawnLocation = transform.position + randomMeteorPosition;
            SpawnMeteorServerRpc(SpawnLocation, 1, spell.Spell_Speed);
            yield return new WaitForSeconds(spell.MultiHitCooldown);

        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SpawnMeteorServerRpc(Vector3 randomMeteorPosition, float randomMeteorSize, float Speed)
    {
        var meteor = Instantiate(Meteor_Prefab, randomMeteorPosition, Quaternion.identity);
        meteor.GetComponent<NetworkObject>().Spawn();
        meteor.GetComponentInChildren<MeteorBehavior>().Initialize(CasterId,new Vector3(0,0,0));
        meteor.transform.localScale = new Vector3(randomMeteorSize, randomMeteorSize, randomMeteorSize);
        meteor.GetComponent<Rigidbody>().AddForce(Vector3.down * Speed, ForceMode.Impulse);
    }

    public void FireSpell()
    {
        StartCoroutine(SpawnMeteors());
        print("should spoawn meteors");
    }

    public void Initialize(ulong casterId, Vector3 direction)
    {
        gameController.GC.PlaySoundAtLocation(transform, spell.FireSound);
        CasterId = casterId;
        var NewDir = direction.normalized;

        //ifHit.point exist then use the position plus  hieght of the meteor
        transform.position += NewDir * 20;
    }

    public override void OnNetworkSpawn()
    {

    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsServer) return;
        if(!CanHit) return;
        if (other.gameObject.TryGetComponent(out IHittable_inherited ihit))
        {
            print(ihit.name + "meteor hit this object");
            if (ihit.Type == IHittable_inherited.ObjectType.player)
            {
                ihit.GotHit(this.gameObject, spell, CasterId);


            }
            var TempLocation = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            /*            var Impact = Instantiate(ImpactEffect, TempLocation, Quaternion.identity);
                        if (Impact != null)
                        {
                            gameController.GC.DestroyObjectOnNetwork(Impact, 5);
                        }
                        Impact.GetComponent<NetworkObject>().Spawn();
            */
           // Destroy(gameObject);
           CanHit = false;
           hitagainTime = 0;
           gameController.GC.DestroyObjectOnNetwork(gameObject, 5);


        }
    }
        public void TriggerEffect()
    {
        throw new System.NotImplementedException();
    }
}
