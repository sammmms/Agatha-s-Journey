using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PauseScript : MonoBehaviour
{
    [SerializeField] private UIDocument pauseMenuUI;
    [SerializeField] private UIDocument equipmentMenuUI;
    private bool isPaused = false;

    void Start()
    {
        if (pauseMenuUI != null)
        {
            // Get buttons by their names
            var exitButton = pauseMenuUI.rootVisualElement.Q<Button>("exitButton");
            var resumeButton = pauseMenuUI.rootVisualElement.Q<Button>("resumeButton");
            var equipmentButton = pauseMenuUI.rootVisualElement.Q<Button>("equipmentButton");

            pauseMenuUI.rootVisualElement.style.display = DisplayStyle.None;

            if (exitButton != null)
                exitButton.clicked += OnExitButtonClicked;
            if (resumeButton != null)
                resumeButton.clicked += OnResumeButtonClicked;
            if (equipmentButton != null)
                equipmentButton.clicked += OnEquipmentButtonClicked;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                OnResumeButtonClicked();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        pauseMenuUI.rootVisualElement.style.display = DisplayStyle.Flex;
        Time.timeScale = 0f;
        isPaused = true;
        UnityEngine.Cursor.visible = true;
        UnityEngine.Cursor.lockState = CursorLockMode.None;
    }



    private void HandleResume()
    {
        pauseMenuUI.rootVisualElement.style.display = DisplayStyle.None;
        Time.timeScale = 1f;
        isPaused = false;


    }

    private void OnExitButtonClicked()
    {
        HandleResume();

        SceneManager.LoadScene("MainMenu");
    }

    private void OnResumeButtonClicked()
    {
        HandleResume();
        UnityEngine.Cursor.visible = false;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEquipmentButtonClicked()
    {
        HandleResume();
        if (equipmentMenuUI != null)
        {
            equipmentMenuUI.rootVisualElement.style.display =
                equipmentMenuUI.rootVisualElement.style.display == DisplayStyle.None ? DisplayStyle.Flex : DisplayStyle.None;
            pauseMenuUI.rootVisualElement.style.display = DisplayStyle.None;
        }
    }
}
