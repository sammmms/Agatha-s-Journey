using UnityEngine;
using UnityEngine.UIElements;

public class InventoryScript : MonoBehaviour
{
    [SerializeField] public UIDocument inventoryUIDocument;
    [SerializeField] private SpellDatabase spellDatabase;
    [SerializeField] private SpellController spellController;
    [SerializeField] private SpellLocomotionInput spellLocomotionInput;


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

        if (spellLocomotionInput != null)
        {
            spellLocomotionInput.enabled = false; // Disable spell locomotion input initially

            spellLocomotionInput.OnSpellSelected += HandleSpellSelected;
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

        UpdateSelectableSpellIcons();

        InitiateHotbarItems(); // Initialize hotbar items
    }

    void HandleSpellSelected(int index)
    {
        if (spellController != null)
        {
            if (index <= 0 || index > spellController.hotbarSpell.Count)
            {
                Debug.LogWarning($"Invalid spell index {index} in HandleSpellSelected.");
                return;
            }

            Spell selectedSpell = spellController.hotbarSpell[index - 1];
            if (selectedSpell == Spell.None)
            {
                Debug.LogWarning("No spell selected in HandleSpellSelected.");
                return;
            }

            HandleHotbarSpellClicked(selectedSpell);
        }
        else
        {
            Debug.LogError("SpellController is not assigned in HandleSpellSelected.");
        }
    }

    void InitiateHotbarItems()
    {
        var root = inventoryUIDocument.rootVisualElement;
        for (int i = 0; i < hotbarItems.Length; i++)
        {
            hotbarItems[i] = root.Q<VisualElement>($"Hotbar_{i + 1}");
        }


        UpdateActiveHotbarSpell(); // Update hotbar spells on start
    }

    void Update()
    {
        HandleEquipmentButtonClicked();
        HandleEscapeKeyPressed();
    }

    void ReflectChanges()
    {
        UpdateSelectableSpellIcons(); // Update spell icons in the UI
        UpdateActiveHotbarSpell(); // Update hotbar spells in the UI
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
            UpdateSelectableSpellIcons();
            UnityEngine.Cursor.visible = true;
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            spellController.enabled = false; // Disable spell controller when UI is open
            spellLocomotionInput.enabled = true; // Disable spell locomotion input when UI is open

            ReflectChanges(); // Reflect changes in the UI
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
            spellController.enabled = true; // Re-enable spell controller when UI is closed
            spellLocomotionInput.enabled = false; // Re-enable spell locomotion input when UI is closed
        }
        else
        {
            Debug.LogError("Inventory UIDocument is not assigned in HandleCloseUI.");
        }
    }

    void UpdateSelectableSpellIcons()
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

    void UpdateActiveHotbarSpell()
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

        ReflectChanges(); // Reflect changes in the UI
    }



    void HandleHotbarSpellClicked(Spell spell)
    {
        if (spell == Spell.None) return;

        spellController.RemoveSpellFromHotbar(spell);

        ReflectChanges(); // Reflect changes in the UI
    }
}