using UnityEngine;
[RequireComponent(typeof(ParticleSystem))]
public class PlaySoundOnParticalImpact : MonoBehaviour

{
    public Spell spell;

    void OnParticleCollision(GameObject other)
    {
        if (spell.ImpactSound != null)
        {
            gameController.GC.PlaySoundAtLocation(transform, spell.ImpactSound);
        }
    }
}
