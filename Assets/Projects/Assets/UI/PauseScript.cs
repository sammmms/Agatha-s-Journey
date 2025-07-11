using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.Collections;

public class PauseScript : MonoBehaviour
{
    [SerializeField] private UIDocument pauseMenuUI;
    [SerializeField] private UIDocument inventoryMenuUI;
    [SerializeField] private AudioClip pauseSound;
    [SerializeField] private AudioClip resumeSound;
    [SerializeField] private AudioSource backgroundAudioSource;
    [SerializeField] private float fadeDuration = 1.0f;

    private bool isPaused = false;
    private Coroutine activeFadeCoroutine;

    // The Start method is a safer place for initialization logic
    // as it runs after all objects have been enabled.
    void Start()
    {
        // Ensure both UI documents are properly referenced and their visual elements are hidden on start.
        if (pauseMenuUI != null && pauseMenuUI.rootVisualElement != null)
        {
            var root = pauseMenuUI.rootVisualElement;
            root.style.display = DisplayStyle.None;

            var exitButton = root.Q<Button>("exitButton");
            var resumeButton = root.Q<Button>("resumeButton");
            var equipmentButton = root.Q<Button>("equipmentButton");

            if (exitButton != null) exitButton.clicked += OnExitButtonClicked;
            if (resumeButton != null) resumeButton.clicked += OnResumeButtonClicked;
            if (equipmentButton != null) equipmentButton.clicked += OnEquipmentButtonClicked;
        }
        else if (pauseMenuUI != null)
        {
            Debug.LogWarning("PauseScript: pauseMenuUI is assigned, but its rootVisualElement is not ready in Start().", this);
        }


        if (inventoryMenuUI != null && inventoryMenuUI.rootVisualElement != null)
        {
            inventoryMenuUI.rootVisualElement.style.display = DisplayStyle.None;
        }
        else if (inventoryMenuUI != null)
        {
            Debug.LogWarning("PauseScript: inventoryMenuUI is assigned, but its rootVisualElement is not ready in Start().", this);
        }
    }

    // OnEnable can be too early for accessing other components' generated content.
    // The logic has been moved to Start(), so this method is no longer needed for this purpose.
    // void OnEnable() { } // You can remove this method entirely.

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Check if the inventory is open. If so, the Escape key should close it first.
            if (inventoryMenuUI != null && inventoryMenuUI.isActiveAndEnabled && inventoryMenuUI.rootVisualElement.style.display == DisplayStyle.Flex)

            {
                inventoryMenuUI.rootVisualElement.style.display = DisplayStyle.None;

                // Close inventory and resume game
                if (isPaused)
                {
                    OnResumeButtonClicked();
                }
            }
            // If not, toggle the pause menu
            else if (isPaused)
            {
                OnResumeButtonClicked();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        if (pauseMenuUI == null || pauseMenuUI.rootVisualElement == null) return;

        pauseMenuUI.rootVisualElement.style.display = DisplayStyle.Flex;
        Time.timeScale = 0f;
        isPaused = true;
        UnityEngine.Cursor.visible = true;
        UnityEngine.Cursor.lockState = CursorLockMode.None;

        if (pauseSound != null)
        {
            AudioSource.PlayClipAtPoint(pauseSound, Camera.main.transform.position);
        }

        if (backgroundAudioSource != null)
        {
            if (activeFadeCoroutine != null) StopCoroutine(activeFadeCoroutine);
            activeFadeCoroutine = StartCoroutine(FadeAudio(0.3f, fadeDuration));
        }

        // This logic is fine here, as it's not part of the initial scene load.
        if (inventoryMenuUI != null)
        {
            inventoryMenuUI.gameObject.SetActive(false);
        }
    }

    private void HandleResume()
    {
        if (pauseMenuUI == null || pauseMenuUI.rootVisualElement == null) return;

        pauseMenuUI.rootVisualElement.style.display = DisplayStyle.None;
        Time.timeScale = 1f;
        isPaused = false;

        if (resumeSound != null)
        {
            AudioSource.PlayClipAtPoint(resumeSound, Camera.main.transform.position);
        }

        if (backgroundAudioSource != null)
        {
            if (activeFadeCoroutine != null) StopCoroutine(activeFadeCoroutine);
            activeFadeCoroutine = StartCoroutine(FadeAudio(0.12f, fadeDuration));
        }

        if (inventoryMenuUI != null)
        {
            inventoryMenuUI.gameObject.SetActive(true);
        }
    }

    private void OnExitButtonClicked()
    {
        Time.timeScale = 1f;
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
        if (pauseMenuUI == null || pauseMenuUI.rootVisualElement == null || inventoryMenuUI == null || inventoryMenuUI.rootVisualElement == null) return;

        pauseMenuUI.rootVisualElement.style.display = DisplayStyle.None;

        // This toggles the inventory and keeps the game paused.
        inventoryMenuUI.rootVisualElement.style.display = DisplayStyle.Flex;

        // No need to call HandleResume here, as we are just swapping menus while still paused.
    }

    private IEnumerator FadeAudio(float targetVolume, float duration)
    {
        if (backgroundAudioSource == null) yield break;

        float startVolume = backgroundAudioSource.volume;
        float time = 0;

        while (time < duration)
        {
            backgroundAudioSource.volume = Mathf.Lerp(startVolume, targetVolume, time / duration);
            time += Time.unscaledDeltaTime;
            yield return null;
        }

        backgroundAudioSource.volume = targetVolume;
    }
}
