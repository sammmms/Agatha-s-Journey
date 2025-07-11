using UnityEngine;

public class DashCooldownScript : MonoBehaviour
{
    [SerializeField] private PlayerStatus _playerStatus;
    [SerializeField] private TMPro.TextMeshProUGUI _cooldownText;
    void Start()
    {
        _cooldownText.text = "Dash Ready";
    }

    // Update is called once per frame
    void Update()
    {
        if (_playerStatus.DashCooldownRemaining > 0)
        {
            _cooldownText.text = $"Dash Cooldown: {_playerStatus.DashCooldownRemaining:F1}s";
        }
        else
        {
            _cooldownText.text = "Dash Ready";
        }
    }
}
