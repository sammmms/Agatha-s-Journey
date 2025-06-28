using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    void Start()
    {
        var root = uiDocument.rootVisualElement;
        
        var startButton = root.Q<Button>("start-button");
        var quitButton = root.Q<Button>("quit-button");

        if (startButton != null)
        {
            startButton.clicked += OnStartButtonClicked;
        }

        if (quitButton != null)
        {
            quitButton.clicked += OnQuitButtonClicked;
        }
    }

    private void OnStartButtonClicked()
    {
        // Load the first game scene (replace "GameScene" with your actual scene name)
        SceneManager.LoadScene("GameScene");
    }

    private void OnQuitButtonClicked()
    {
        // Quit the application
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    void Update()
    {
        
    }
}
