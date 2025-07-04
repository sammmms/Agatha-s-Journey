using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HotbarScript : MonoBehaviour
{
    public UIDocument uiDocument;
    public SpellDatabase spellDatabase;
    public SpellController spellController; // Reference to SpellController
    public int maxSpells = 4;

    private List<Spell> currentHotbarSpells = new List<Spell>();

    private VisualElement[] hotbarItems = new VisualElement[4];

    void Awake()
    {
        if (uiDocument == null)
        {
            Debug.LogError("UIDocument is not assigned in HotbarScript.");
            return;
        }

        if (spellDatabase == null)
        {
            Debug.LogError("SpellDatabase is not assigned in HotbarScript.");
            return;
        }

        var root = uiDocument.rootVisualElement;
        for (int i = 0; i < maxSpells; i++)
        {
            currentHotbarSpells.Add(Spell.None);
            hotbarItems[i] = root.Q<VisualElement>($"HotbarItems_{i + 1}");
        }

    }

    void Update()
    {
        UpdateHotbarSpells();
        UpdateActiveSpellMark();
    }

    private void UpdateHotbarSpells()
    {
        var activeHotbarSpells = spellController.hotbarSpell;
        for (int i = 0; i < maxSpells; i++)
        {
            Spell newSpell = (i < activeHotbarSpells.Count) ? activeHotbarSpells[i] : Spell.None;
            if (currentHotbarSpells[i] != newSpell)
            {
                currentHotbarSpells[i] = newSpell;
                UpdateHotbarSlot(i, newSpell);
            }
        }
    }

    private void UpdateHotbarSlot(int slot, Spell spell)
    {
        if (hotbarItems[slot] == null) return;

        hotbarItems[slot].Clear();

        if (spell == Spell.None)
        {
            return;
        }

        GameObject spellPrefab = spellDatabase.GetSpellPrefab(spell);
        if (spellPrefab != null)
        {
            if (spellPrefab.TryGetComponent(out BaseSpellAttribute spellAttribute) && spellAttribute.spellIcon != null)
            {
                var iconElement = new VisualElement();
                iconElement.AddToClassList("hotbar_item");
                iconElement.style.backgroundImage = spellAttribute.spellIcon.texture;
                iconElement.tooltip = spellAttribute.spell.ToString();

                Debug.Log($"Adding spell {spell} to hotbar slot {slot} with icon {spellAttribute.spellIcon.name}");
                hotbarItems[slot].Add(iconElement);
            }
            else
            {
                Debug.LogWarning($"Spell {spell} does not have a BaseSpellAttribute component or spellIcon.");
            }
        }
    }

    private void UpdateActiveSpellMark()
    {
        List<Spell> activeSpells = spellController.activeSpells;
        for (int i = 0; i < maxSpells; i++)
        {
            if (currentHotbarSpells[i] != Spell.None)
            {
                if (activeSpells.Contains(currentHotbarSpells[i]))
                {
                    hotbarItems[i]?.AddToClassList("hotbar_active");
                }
                else
                {
                    hotbarItems[i]?.RemoveFromClassList("hotbar_active");
                }
            }
            else
            {
                hotbarItems[i]?.RemoveFromClassList("hotbar_active");
            }
        }
    }
}
