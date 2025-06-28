using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class StaminaBar : MonoBehaviour
{
    public Slider slider;

    public void SetStamina(float stamina)
    {
        print($"Stamina: {stamina}");
        slider.value = stamina;
    }

    public void SetMaxStamina(float maxStamina)
    {
        print($"SetMaxStamina : {maxStamina}");
        slider.maxValue = maxStamina;
        slider.value = maxStamina;
    }


}
