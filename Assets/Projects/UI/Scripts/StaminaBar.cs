using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class StaminaBar : MonoBehaviour
{
    public Slider slider;

    public void SetStamina(float stamina)
    {
        slider.value = stamina;
    }

    public void SetMaxStamina(float maxStamina)
    {
        slider.maxValue = maxStamina;
        slider.value = maxStamina;
    }


}
