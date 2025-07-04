using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PauseScript : MonoBehaviour
{
    [SerializeField] private UIDocument pauseMenuUI;
    [SerializeField] private UIDocument inventoryMenuUI;
    [SerializeField] private AudioClip pauseSound;
    [SerializeField] private AudioClip resumeSound;
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

    void OnEnable()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.rootVisualElement.style.display = DisplayStyle.None;
        }

        if (inventoryMenuUI != null)
        {
            inventoryMenuUI.rootVisualElement.style.display = DisplayStyle.None;
        }
    }

    public void PauseGame()
    {
        if (inventoryMenuUI != null && inventoryMenuUI.rootVisualElement.style.display != DisplayStyle.None)
        {
            return;
        }

        pauseMenuUI.rootVisualElement.style.display = DisplayStyle.Flex;
        Time.timeScale = 0f;
        isPaused = true;
        UnityEngine.Cursor.visible = true;
        UnityEngine.Cursor.lockState = CursorLockMode.None;

        if (pauseSound != null)
        {
            AudioSource.PlayClipAtPoint(pauseSound, Camera.main.transform.position);
        }


        if (inventoryMenuUI != null)
        {
            inventoryMenuUI.gameObject.SetActive(false);
        }
    }



    private void HandleResume()
    {
        pauseMenuUI.rootVisualElement.style.display = DisplayStyle.None;
        Time.timeScale = 1f;
        isPaused = false;

        if (resumeSound != null)
        {
            AudioSource.PlayClipAtPoint(resumeSound, Camera.main.transform.position);
        }


        if (inventoryMenuUI != null)
        {
            inventoryMenuUI.gameObject.SetActive(true);
            inventoryMenuUI.rootVisualElement.style.display = DisplayStyle.None;
        }
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
        if (inventoryMenuUI != null)
        {
            inventoryMenuUI.rootVisualElement.style.display =
                inventoryMenuUI.rootVisualElement.style.display == DisplayStyle.None ? DisplayStyle.Flex : DisplayStyle.None;
            pauseMenuUI.rootVisualElement.style.display = DisplayStyle.None;
        }
    }
}
