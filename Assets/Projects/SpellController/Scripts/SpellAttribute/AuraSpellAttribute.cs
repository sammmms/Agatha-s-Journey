using UnityEngine;

abstract public class AuraSpellAttribute : BaseSpellAttribute
{
    public abstract void cancelSpell(PlayerController playerController);
}