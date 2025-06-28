using UnityEngine;
using UnityEngine.UIElements;

public class PauseScript : MonoBehaviour
{
    [SerializeField] private UIDocument pauseMenuUI;
    private bool isPaused = false;

    void Start()

    {   
        Debug.Log("PauseScript: Start method called, initializing pause menu UI.");
        // Get the UI Document component
        
        // Ensure pause menu is hidden at start
        if (pauseMenuUI != null)
            pauseMenuUI.gameObject.SetActive(false);
    }

    void Update()
    {
        Debug.Log("PauseScript: Update method called, checking for pause input.");
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("PauseScript: Escape key pressed, toggling pause state.");
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        pauseMenuUI.gameObject.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        pauseMenuUI.gameObject.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

}