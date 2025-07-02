using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class ModeSelectionScript : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    void Awake()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        var root = uiDocument.rootVisualElement;

        var leftSelection = root.Q<VisualElement>("LeftSelection");
        var rightSelection = root.Q<VisualElement>("RightSelection");
        var screenBackground = root.Q<VisualElement>("ScreenBackground");

        if (leftSelection != null)
        {
            leftSelection.RegisterCallback<ClickEvent>(evt =>
            {
                // Do nothing
            });
        }

        if (rightSelection != null)
        {
            rightSelection.RegisterCallback<ClickEvent>(evt =>
            {
                SceneManager.LoadScene("LoadingScene");
            });
        }

        if (screenBackground != null)
        {
            screenBackground.RegisterCallback<ClickEvent>(evt =>
            {
                gameObject.SetActive(false);
            });
        }
    }
}
