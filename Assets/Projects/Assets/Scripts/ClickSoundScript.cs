using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ClickSoundScript : MonoBehaviour
{
    [SerializeField] private UIDocument uiDoc;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private List<string> classNames = new List<string> { "menu-button" };

    void Start()
    {
        var root = uiDoc.rootVisualElement;

        foreach (var className in classNames)
        {
            var buttons = root.Query<VisualElement>(className: className).ToList();
            foreach (var btn in buttons)
            {
                btn.RegisterCallback<ClickEvent>((evt) =>
                {
                    if (audioSource != null)
                    {
                        if (clickSound != null)
                        {
                            audioSource.clip = clickSound;
                        }
                        audioSource.Play();
                    }
                });
            }
        }
    }
}
