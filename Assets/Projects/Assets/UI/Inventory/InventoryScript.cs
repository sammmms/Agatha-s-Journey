using UnityEngine;
using UnityEngine.UIElements;

public class InventoryScript : MonoBehaviour
{
    [SerializeField] public UIDocument inventoryUIDocument;
    [SerializeField] private SpellDatabase spellDatabase;
    [SerializeField] private SpellController spellController;

    private VisualElement equipmentCard;

    private VisualElement[] hotbarItems = new VisualElement[4];

    void Awake()
    {
        if (inventoryUIDocument == null)
        {
            Debug.LogError("Inventory UIDocument is not assigned.");
            return;
        }

        if (spellDatabase == null)
        {
            Debug.LogError("SpellDatabase is not assigned.");
            return;
        }

    }

    void Start()
    {
        // Ensure the inventory UI is initially hidden
        if (inventoryUIDocument != null)
        {
            inventoryUIDocument.rootVisualElement.style.display = DisplayStyle.None;
        }
        else
        {
            Debug.LogError("Inventory UIDocument is not assigned in Start.");
        }
    }

    void OnEnable()
    {
        var root = inventoryUIDocument.rootVisualElement;

        equipmentCard = root.Q<VisualElement>("EquipmentCard");
        if (equipmentCard == null)
        {
            Debug.LogError("EquipmentCard element not found in the Inventory UI.");
            return;
        }

        root.style.display = DisplayStyle.None; // Ensure the UI is hidden initially

        UpdateSpellIcons();

        InitiateHotbarItems(); // Initialize hotbar items
    }

    void InitiateHotbarItems()
    {
        var root = inventoryUIDocument.rootVisualElement;
        for (int i = 0; i < hotbarItems.Length; i++)
        {
            hotbarItems[i] = root.Q<VisualElement>($"Hotbar_{i + 1}");
        }


        HandleActiveHotbarSpell(); // Update hotbar spells on start
    }

    void Update()
    {
        HandleEquipmentButtonClicked();
        HandleEscapeKeyPressed();
    }

    void HandleEquipmentButtonClicked()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (inventoryUIDocument.rootVisualElement.style.display == DisplayStyle.None)
            {
                HandleOpenUI();
            }
            else
            {
                HandleCloseUI();
            }
        }
    }

    void HandleEscapeKeyPressed()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleCloseUI();
        }
    }

    private void HandleOpenUI()
    {
        if (inventoryUIDocument != null)
        {
            Time.timeScale = 0f;
            inventoryUIDocument.rootVisualElement.style.display = DisplayStyle.Flex;
            UpdateSpellIcons();
            UnityEngine.Cursor.visible = true;
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            HandleActiveHotbarSpell(); // Update hotbar spells when opening the UI
        }
        else
        {
            Debug.LogError("Inventory UIDocument is not assigned in HandleOpenUI.");
        }
    }

    private void HandleCloseUI()
    {
        if (inventoryUIDocument != null)
        {
            Time.timeScale = 1f;
            inventoryUIDocument.rootVisualElement.style.display = DisplayStyle.None;
            UnityEngine.Cursor.visible = false;
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Debug.LogError("Inventory UIDocument is not assigned in HandleCloseUI.");
        }
    }

    void UpdateSpellIcons()
    {
        if (equipmentCard == null) return;
        equipmentCard.Clear();

        foreach (GameObject spell in spellDatabase.GetAllBasicSpellsPrefab())
        {
            BaseSpellAttribute spellAttribute = spell.GetComponent<BaseSpellAttribute>();
            var spellElement = new VisualElement();
            spellElement.style.backgroundImage = new StyleBackground(spellAttribute.spellIcon);
            spellElement.AddToClassList("items");
            if (IsSpellInHotbar(spellAttribute.spell))
            {
                spellElement.AddToClassList("disable");
            }
            else
            {
                spellElement.RegisterCallback<ClickEvent>(evt => HandleSpellClick(spellAttribute.spell));

            }
            equipmentCard.Add(spellElement);
        }
    }

    bool IsSpellInHotbar(Spell spell)
    {
        return spellController.hotbarSpell.Contains(spell);
    }

    void HandleSpellClick(Spell spell)
    {
        if (spell == Spell.None) return;

        // Check if the spell is already in the hotbar
        if (IsSpellInHotbar(spell))
        {
            Debug.Log($"Spell {spell} is already in the hotbar.");
            return;
        }

        // Add the spell to the hotbar
        spellController.AddSpellToHotbar(spell);
        UpdateSpellIcons();

        // Update the hotbar UI to reflect the new spell
        HandleActiveHotbarSpell();
    }

    void HandleActiveHotbarSpell()
    {
        var root = inventoryUIDocument.rootVisualElement;
        for (int i = 0; i < hotbarItems.Length; i++)
        {
            var hotbarItem = hotbarItems[i];
            if (hotbarItem == null) continue;

            // Clear previous content
            hotbarItem.Clear();

            // Get the spell for this hotbar slot
            Spell spell = spellController.hotbarSpell.Count > i ? spellController.hotbarSpell[i] : Spell.None;
            if (spell == Spell.None)
            {
                continue; // Skip to the next slot if no spell is assigned
            }

            // Create a new visual element for the spell
            var spellElement = new VisualElement();
            BaseSpellAttribute spellAttribute = spellDatabase.GetSpellPrefab(spell).GetComponent<BaseSpellAttribute>();
            spellElement.style.backgroundImage = new StyleBackground(spellAttribute.spellIcon);
            spellElement.AddToClassList("hotbar-item");
            spellElement.RegisterCallback<ClickEvent>(evt => HandleHotbarSpellClicked(spell));


            // Add the spell element to the hotbar item
            hotbarItem.Add(spellElement);
        }
    }

    void HandleHotbarSpellClicked(Spell spell)
    {
        if (spell == Spell.None) return;

        spellController.RemoveSpellFromHotbar(spell);
        UpdateSpellIcons();

        // Update the hotbar UI to reflect the removed spell
        HandleActiveHotbarSpell();
    }
}