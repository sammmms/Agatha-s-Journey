using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpellLocomotionInput : MonoBehaviour, SpellControls.IItemInputsActions
{

    public SpellControls SpellControls { get; private set; }

    public event Action<int> OnSpellSelected;
    public event Action OnShootTriggered;


    public int SpellIndex { get; private set; }

    private void OnEnable()
    {
        SpellControls = new SpellControls();
        SpellControls.Enable();

        SpellControls.ItemInputs.Enable();
        SpellControls.ItemInputs.SetCallbacks(this);
    }

    private void OnDisable()
    {
        SpellControls.ItemInputs.Disable();
        SpellControls.ItemInputs.RemoveCallbacks(this);
    }

    public void OnShootInputs(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Left click - auto target
            if (context.control.name == "leftButton")
            {
                OnShootTriggered?.Invoke();
            }
        }
    }

    public void OnToggleInputs(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Get the pressed key (1-4)
            if (int.TryParse(context.control.name, out int keyNumber))
            {
                // Toggle spell selection (if same key pressed twice, deselect)
                SpellIndex = keyNumber;
                OnSpellSelected?.Invoke(SpellIndex);
            }
        }
    }
}
