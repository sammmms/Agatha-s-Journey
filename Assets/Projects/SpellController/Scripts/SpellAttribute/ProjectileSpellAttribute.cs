using UnityEngine;

public class ProjectileSpellAttribute : BaseSpellAttribute
{
    public float spellSpeed = 10f;
    public float spellDamage;
    public float stunDuration;
    public float stunPerInstance;

    public override GameObject CastSpell(PlayerController playerController)
    {
        ProjectileShooter projectileShooter = playerController.GetComponent<ProjectileShooter>();

        projectileShooter.LaunchProjectile(spellPrefab);

        return null;
    }
}