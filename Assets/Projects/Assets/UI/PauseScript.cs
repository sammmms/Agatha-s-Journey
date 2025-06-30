using UnityEngine;
using UnityEngine.UIElements;

public class PauseScript : MonoBehaviour
{
    [SerializeField] private UIDocument pauseMenuUI;
    private bool isPaused = false;

    void Start()

    {

        if (pauseMenuUI != null)
            pauseMenuUI.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
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