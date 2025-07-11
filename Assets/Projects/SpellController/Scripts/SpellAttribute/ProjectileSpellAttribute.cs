using UnityEngine;

public class ProjectileSpellAttribute : DamagingSpellAttribute
{
    public float spellSpeed = 10f;
    public float stunDuration;
    public float stunPerInstance;
    public bool isTracking = false;

    protected override GameObject TriggerSpell()
    {
        if (spellCaster.TryGetComponent<ProjectileShooter>(out var projectileShooter))
        {
            if (PlayerStatus != null)
            {
                spellDamage += PlayerStatus.damageBuff;
            }
            projectileShooter.LaunchProjectile(spellPrefab);
        }

        return null;
    }
}