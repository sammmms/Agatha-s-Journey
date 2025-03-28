using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class ManaBar : MonoBehaviour
{
    public Slider slider;

    public void SetMana(float mana)
    {

        print($"SetMana : {mana}");
        slider.value = mana;
    }

    public void SetMaxMana(float maxMana)
    {
        print($"SetMaxMana : {maxMana}");
        slider.maxValue = maxMana;
        slider.value = maxMana;
    }


}
