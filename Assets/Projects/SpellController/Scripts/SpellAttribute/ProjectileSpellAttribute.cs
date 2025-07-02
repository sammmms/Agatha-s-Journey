using UnityEngine;

public class ProjectileSpellAttribute : BaseSpellAttribute
{
    public float spellSpeed = 10f;
    public float spellDamage;
    public float stunDuration;
    public float stunPerInstance;

    protected override GameObject TriggerSpell()
    {
        if (spellCaster.TryGetComponent<ProjectileShooter>(out var projectileShooter))
        {
            projectileShooter.LaunchProjectile(spellPrefab);
        }

        return null;
    }
}