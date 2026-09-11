using UnityEngine;
using UnityEngine.UI;

public class HealthBarBehaviour : MonoBehaviour
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
        
    }

    // Update is called once per frame
    void Update()
    {
        healthBarSlider.value = playerBehaviour.Health;
    }
}
