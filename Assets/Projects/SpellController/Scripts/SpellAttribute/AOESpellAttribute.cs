using UnityEngine;

public class AreaOfEffectSpellAttribute : BaseSpellAttribute
{
    public float impactDamage;
    public float impactRadius;
    public float impactStunDuration;
    public float impactStun;
    public float spellDuration;

    public override GameObject castSpell(PlayerController playerController)
    {
        AOEInvoker aoeInvoker = playerController.GetComponent<AOEInvoker>();

        if (aoeInvoker != null)
        {
            aoeInvoker.CastAOESpell(spellPrefab);
            return spellPrefab;
        }
        else
        {
            Debug.LogError("AOEInvoker component not found on PlayerController.");
        }

        return null;
    }
}