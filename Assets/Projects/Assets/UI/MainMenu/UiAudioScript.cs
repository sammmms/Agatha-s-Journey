using UnityEngine;
using UnityEngine.UIElements;

public class HoverSound : MonoBehaviour
{
    [SerializeField] private UIDocument uiDoc;
    [SerializeField] private AudioSource audioSource;
    private AudioClip hoverSound;

    void Start()
    {
        // Load sound from Resources/Sounds/hover.wav
        hoverSound = Resources.Load<AudioClip>("Sounds/hover");
        if (hoverSound == null)
        {
            Debug.LogError("Hover sound not found! Put it in Resources/Sounds/hover.wav");
            return;
        }

        var root = uiDoc.rootVisualElement;
        var buttons = root.Query<VisualElement>(className: "menu-button").ToList();

        foreach (var btn in buttons)
        {
            btn.RegisterCallback<MouseEnterEvent>((evt) =>
            {
                if (audioSource != null && hoverSound != null)
                {
                    audioSource.PlayOneShot(hoverSound);
                }
            });
        }
    }
}
