using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{

    [SerializeField] Slider healthBarSlider;
    [SerializeField] Enemy enemy;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2f, 0f);
    [SerializeField] private Camera targetCamera;

    private Canvas parentCanvas;

    private void Awake()
    {
        if (enemy == null)
            enemy = GetComponentInParent<Enemy>();

        healthBarSlider.value = 0;
        parentCanvas = healthBarSlider.GetComponentInParent<Canvas>();
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (enemy != null)
        {
            healthBarSlider.value = enemy.Health;
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (enemy == null || healthBarSlider == null) return;

        // Met à jour la valeur
        healthBarSlider.value = Mathf.Clamp01(enemy.Health / 100f);

        Vector3 worldPos = enemy.transform.position + worldOffset;

        // Si le canvas est en World Space, positionne directement dans le monde
        if (parentCanvas != null && parentCanvas.renderMode == RenderMode.WorldSpace)
        {
            healthBarSlider.transform.position = worldPos;
        }
        else
        {
            // Canvas en Screen Space (Overlay ou Camera) : convertir la position monde en position écran
            Camera cam = targetCamera != null ? targetCamera : Camera.main;
            if (cam != null)
            {
                Vector3 screenPoint = cam.WorldToScreenPoint(worldPos);

                // Si le canvas utilise des RectTransform parents, il est plus sûr d'assigner la position en espace écran
                healthBarSlider.transform.position = screenPoint;
            }
            else
            {
                // Fallback: positionne en monde
                healthBarSlider.transform.position = worldPos;
            }
        }
    }
}
