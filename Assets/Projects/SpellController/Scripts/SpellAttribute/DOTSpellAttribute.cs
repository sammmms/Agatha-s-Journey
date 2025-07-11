using UnityEngine;

public class DamageOverTimeSpellAttribute : DamagingSpellAttribute
{
    public float dotDamage;
    public float dotDuration;
    public float dotTick;

    protected override GameObject TriggerSpell()
    {
        throw new System.NotImplementedException();
    }
}





