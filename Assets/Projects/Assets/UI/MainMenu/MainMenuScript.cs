using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private UIDocument menuSelectorDocument;

    void Start()
    {
        var root = uiDocument.rootVisualElement;

        var startButton = root.Q<Button>("start-button");
        var quitButton = root.Q<Button>("quit-button");

        if (startButton != null)
        {
            startButton.clicked += () =>
            {
                OnStartButtonClicked();
            };
        }

        if (quitButton != null)
        {
            quitButton.clicked += () =>
            {
                OnQuitButtonClicked();
            };
        }
    }

    private void OnStartButtonClicked()
    {
        menuSelectorDocument.gameObject.SetActive(true);
    }

    private void OnQuitButtonClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}
