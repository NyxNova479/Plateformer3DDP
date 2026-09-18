using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthbar : MonoBehaviour
{

    [SerializeField] Slider healthBarSlider;
    [SerializeField] PlayerBehaviour playerBehaviour;

    private void Awake()
    {
        playerBehaviour = FindAnyObjectByType<PlayerBehaviour>();
        healthBarSlider.value = 0;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (playerBehaviour != null)
        {
            healthBarSlider.value = Mathf.Clamp01(playerBehaviour.Health / 100f);
            // Subscribe to player's health events if available
            playerBehaviour.OnHealthChanged += OnPlayerHealthChanged;
            playerBehaviour.OnDeath += OnPlayerDeath;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (playerBehaviour != null && healthBarSlider != null)
        {
            healthBarSlider.value = Mathf.Clamp01(playerBehaviour.Health / 100f);
        }
    }

    private void OnDestroy()
    {
        if (playerBehaviour != null)
        {
            playerBehaviour.OnHealthChanged -= OnPlayerHealthChanged;
            playerBehaviour.OnDeath -= OnPlayerDeath;
        }
    }

    private void OnPlayerHealthChanged(float newHealth)
    {
        if (healthBarSlider != null)
            healthBarSlider.value = Mathf.Clamp01(newHealth / 100f);
    }

    private void OnPlayerDeath()
    {
        if (healthBarSlider != null)
            Destroy(healthBarSlider.gameObject);
    }
}
