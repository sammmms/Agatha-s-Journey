using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class BossBarScript : MonoBehaviour
{
    [Header("Boss Health Bar")]
    [SerializeField] private GameObject _bossHealthBar;
    [SerializeField] private TextMeshProUGUI _bossNameText; // Optional: Text to display the boss name
    [SerializeField] private TextMeshProUGUI _bossHealthText; // Optional: Text to display the boss health percentage
    [SerializeField] private List<string> _enemyTags = new() { "Enemies" }; // Tags to identify enemies

    Slider _slider;
    GameObject _enemy;
    bool _isAnimating = false;

    void Awake()
    {
        _slider = _bossHealthBar.GetComponent<Slider>();
        if (_slider == null)
        {
            Debug.LogError("Slider component not found on Boss Health Bar.");
        }
    }

    void Update()
    {
        if (_enemy == null)
        {
            // Find the first enemy in the scene with the specified tags
            foreach (string tag in _enemyTags)
            {
                GameObject[] foundEnemies = GameObject.FindGameObjectsWithTag(tag);
                foreach (GameObject foundEnemy in foundEnemies)
                {
                    EnemyStatus enemyStatus = foundEnemy.GetComponent<EnemyStatus>();
                    if (enemyStatus != null && !enemyStatus.IsDead)
                    {
                        _enemy = foundEnemy;
                        break; // Exit inner loop once an enemy is found
                    }
                }
                if (_enemy != null)
                    break; // Exit outer loop if an enemy has been assigned
            }
        }

        if (_enemy != null)
        {
            HandleEnemyHealth();
        }
        else
        {
            HandleNoEnemy();
        }
    }

    void HandleNoEnemy()
    {
        if (_enemy == null)
        {
            _slider.value = 0f; // Reset the slider if no enemy is found
            _isAnimating = false;

            if (_bossHealthBar.activeSelf)
            {
                _bossHealthBar.SetActive(false); // Hide the health bar if no enemy is present
            }

            if (_bossNameText != null)
            {
                _bossNameText.text = ""; // Clear the boss name text
            }

            if (_bossHealthText != null)
            {
                _bossHealthText.text = ""; // Clear the boss health text
            }
        }
    }

    void HandleEnemyHealth()
    {
        if (_bossHealthBar.activeSelf == false)
        {
            _bossHealthBar.SetActive(true); // Show the health bar if it was hidden
        }

        if (_bossNameText != null)
        {
            _bossNameText.text = _enemy.name; // Update the boss name text
        }

        if (_enemy.TryGetComponent<EnemyStatus>(out var enemyStatus))
        {
            float currentHealth = enemyStatus.CurrentHealth;
            float maxHealth = enemyStatus.MaxHealth;

            if (_bossHealthText != null)
            {
                _bossHealthText.text = $"{currentHealth}/{maxHealth}"; // Update the boss health text
            }

            if (!_isAnimating)
            {
                StartCoroutine(AnimateSlider(currentHealth / maxHealth));
            }
        }
        else
        {
            Debug.LogWarning("EnemyStatus component not found on the enemy.");
        }
    }

    IEnumerator AnimateSlider(float targetValue)
    {
        _isAnimating = true;
        float duration = 0.3f;
        float elapsed = 0f;
        float startValue = _slider.value;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _slider.value = Mathf.Lerp(startValue, targetValue, elapsed / duration);
            yield return null;
        }
        _slider.value = targetValue;
        _isAnimating = false;
    }
}
