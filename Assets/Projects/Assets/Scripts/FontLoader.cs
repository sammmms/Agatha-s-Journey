using UnityEngine;
using UnityEngine.UIElements;

public class FontLoader : MonoBehaviour
{
    [SerializeField] private UIDocument uiDoc;

    void Start()
    {
        var font = Resources.Load<Font>("Fonts/Times");
        if (font == null)
        {
            Debug.LogError("Font not found! Make sure Times.ttf is in Resources/Fonts/");
            return;
        }

        var root = uiDoc.rootVisualElement;
        var buttons = root.Query<VisualElement>(className: "menu-button").ToList();

        foreach (var btn in buttons)
        {
            btn.style.unityFont = font;
            btn.style.unityFontDefinition = new StyleFontDefinition(font);
            btn.MarkDirtyRepaint(); // Force redraw
        }
    }
}
