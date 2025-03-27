using UnityEngine;

public class ProjectileSpellAttribute : BaseSpellAttribute
{
    public float spellSpeed;
    public float spellDamage;
    public float stunDuration;
    public float stunPerInstance;

    public override bool canCastSpell(float currentCooldown, float currentMana)
    {
        return currentCooldown >= spellCooldown && currentMana >= spellCost;
    }

    public override GameObject castSpell()
    {
        ProjectileShooter projectileShooter = GetComponent<ProjectileShooter>();

        projectileShooter.LaunchProjectile(spellPrefab);

        return null;
    }
}