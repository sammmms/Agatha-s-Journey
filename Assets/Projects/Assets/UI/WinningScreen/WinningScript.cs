using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class WinningScript : MonoBehaviour
{
    [SerializeField] private EnemyStatus enemyStatus;
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private List<GameObject> objectToDisable;

    private VisualElement root;
    private Label title;
    private Label subtitle;
    private Button exitButton;
    private Button retryButton;

    private bool uiShown = false;

    void Start()
    {
        root = uiDocument.rootVisualElement;
        exitButton = root.Q<Button>("ExitButton");
        retryButton = root.Q<Button>("RetryButton");
        title = root.Q<Label>("Title");
        subtitle = root.Q<Label>("Subtitle");

        root.style.display = DisplayStyle.None;

        exitButton.clicked += OnExitClicked;
        retryButton.clicked += OnRetryClicked;
    }

    void Update()
    {
        if (enemyStatus.IsDead && !uiShown)
        {
            ShowUI();
        }
    }

    void ShowUI()
    {
        root.style.display = DisplayStyle.Flex;
        title.text = "Congratulations!";
        subtitle.text = "You have defeated the bosses!";
        uiShown = true;

        UnityEngine.Cursor.visible = true;
        UnityEngine.Cursor.lockState = CursorLockMode.None;

        // Disable other UI documents
        foreach (var obj in objectToDisable)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }

        // Disable camera movement script(s)
        var camera = Camera.main;
        if (camera != null)
        {
            if (camera.TryGetComponent<MonoBehaviour>(out var cameraMovement))
            {
                cameraMovement.enabled = false;
            }
        }
    }

    void HideUI()
    {
        root.style.display = DisplayStyle.None;
        uiShown = false;


        foreach (var obj in objectToDisable)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }

    }

    private void OnExitClicked()
    {
        HideUI();
        SceneManager.LoadScene("MainMenu");
    }

    private void OnRetryClicked()
    {
        HideUI();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


}
