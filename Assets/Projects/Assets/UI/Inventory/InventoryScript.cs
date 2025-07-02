using UnityEngine;
using UnityEngine.UIElements;

public class InventoryScript : MonoBehaviour
{
    [SerializeField] public UIDocument inventoryUIDocument;
    [SerializeField] private SpellDatabase spellDatabase;

    private VisualElement equipmentCard;

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

        UpdateSpellIcons();

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
                inventoryUIDocument.rootVisualElement.style.display = DisplayStyle.Flex;
                UpdateSpellIcons();
            }
            else
            {
                inventoryUIDocument.rootVisualElement.style.display = DisplayStyle.None;
            }
        }
    }

    void HandleEscapeKeyPressed()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Close the inventory UI
            inventoryUIDocument.rootVisualElement.style.display = DisplayStyle.None;


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
            spellElement.AddToClassList("items");
            spellElement.style.backgroundImage = new StyleBackground(spellAttribute.spellIcon);
            spellElement.RegisterCallback<ClickEvent>(evt => HandleSpellClick(spellElement));
            equipmentCard.Add(spellElement);
        }
    }

    void HandleSpellClick(VisualElement spellElement)
    {
        Debug.Log("Spell clicked: " + spellElement.name);
    }
}