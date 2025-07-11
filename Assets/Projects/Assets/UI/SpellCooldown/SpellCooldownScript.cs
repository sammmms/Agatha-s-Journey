using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SpellCooldownScript : MonoBehaviour
{
    [SerializeField] UIDocument spellCooldownUIDocument;
    [SerializeField] SpellController spellController;
    [SerializeField] SpellDatabase spellDatabase;

    VisualElement spellContainer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (spellController == null)
        {
            Debug.LogError("SpellController is not assigned in SpellCooldownScript.");
            return;
        }

        if (spellDatabase == null)
        {
            Debug.LogError("SpellDatabase is not assigned in SpellCooldownScript.");
            return;
        }

        if (spellCooldownUIDocument == null)
        {
            Debug.LogError("SpellCooldown UIDocument is not assigned in SpellCooldownScript.");
            return;
        }

        spellContainer = spellCooldownUIDocument.rootVisualElement.Q<VisualElement>("SpellCooldownContainer");
    }

    // Update is called once per frame
    void Update()
    {
        if (spellController == null || spellDatabase == null)
        {
            Debug.LogError("SpellController or SpellDatabase is not assigned in Update.");
            return;
        }

        if (spellContainer == null)
        {
            Debug.LogError("SpellCooldownContainer is not found in the UIDocument.");
            return;
        }

        // Remove all children from SpellCooldownContainer
        spellContainer.Clear();

        // Loop through every spell cooldown and render those that should be displayed
        foreach (var spell in spellController.spellCooldown)
        {
            GameObject spellObject = spellDatabase.GetSpellPrefab(spell.Key);
            BaseSpellAttribute spellAttribute = spellObject?.GetComponent<BaseSpellAttribute>();

            if (spellObject != null)
            {
                // Create a new container for this spell
                var spellItemContainer = new VisualElement();
                spellItemContainer.AddToClassList("spell-container");
                spellItemContainer.style.backgroundImage = spellAttribute?.spellIcon.texture;

                // Add cooldown label
                var cooldownLabel = new Label();
                cooldownLabel.name = "CooldownTime";
                float remainingCooldown = Mathf.Round(spell.Value);
                cooldownLabel.text = remainingCooldown % 1 == 0 ? $"{(int)remainingCooldown}s" : $"{remainingCooldown:F1}s";
                cooldownLabel.AddToClassList("cooldown-label");
                spellItemContainer.Add(cooldownLabel);

                // Show only if cooldown is active
                spellItemContainer.style.display = remainingCooldown > 0 ? DisplayStyle.Flex : DisplayStyle.None;

                // Add to SpellCooldownContainer
                spellContainer.Add(spellItemContainer);
            }
        }
    }
}
