using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HoverSoundScript : MonoBehaviour
{
    [SerializeField] private UIDocument uiDoc;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private List<string> classNames = new List<string> { "menu-button" };

    void Start()
    {
        var root = uiDoc.rootVisualElement;

        foreach (var className in classNames)
        {
            var buttons = root.Query<VisualElement>(className: className).ToList();
            foreach (var btn in buttons)
            {
                btn.RegisterCallback<MouseEnterEvent>((evt) =>
                {
                    if (audioSource != null)
                    {
                        audioSource.Play();
                    }
                });
            }
        }
    }
}
