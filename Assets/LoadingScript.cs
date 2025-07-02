using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement; // Add this for scene loading

public class LoadingScript : MonoBehaviour
{
    public Slider loadingSlider;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI loadingSentenceText;

    private float duration = 3f;
    private float timer = 0f;

    private string[] loadingSentences = new string[]
    {
        "Loading magical forests...",
        "Summoning friendly creatures...",
        "Brewing potions...",
        "Polishing Agatha's boots...",
        "Preparing adventures...",
        "Counting stars...",
        "Feeding the owls...",
        "Sharpening wands..."
    };

    void Start()
    {
        if (loadingSlider != null)
            loadingSlider.value = 0f;

        UpdateProgressText(0f);
        UpdateLoadingSentence(0f);
    }

    void Update()
    {
        if (loadingSlider != null && loadingSlider.value < 1f)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / duration);
            loadingSlider.value = progress;
            UpdateProgressText(progress);
            UpdateLoadingSentence(progress);

            if (progress >= 1f)
            {
                SceneManager.LoadScene("GameScene");
            }
        }
    }

    void UpdateProgressText(float progress)
    {
        if (progressText != null)
            progressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
    }

    void UpdateLoadingSentence(float progress)
    {
        if (loadingSentenceText != null && loadingSentences.Length > 0)
        {
            int index = Mathf.FloorToInt(progress * (loadingSentences.Length - 1));
            loadingSentenceText.text = loadingSentences[index];
        }
    }
}
