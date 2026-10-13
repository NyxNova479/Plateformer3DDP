using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Nécessaire pour recharger la scène
using TMPro;
using System.Collections;

public class LevelVictoryManager : MonoBehaviour
{
    public static LevelVictoryManager Instance { get; private set; }

    [Header("UI Victory Panel")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private Image fadeImage;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private float fadeDuration = 1.5f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(false);
        }
    }

    public void TriggerVictory()
    {
        StartCoroutine(VictorySequenceRoutine());
    }

    private IEnumerator VictorySequenceRoutine()
    {
        Debug.Log("Victoire ! Déclenchement du fondu au noir et du récapitulatif.");

        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            float elapsed = 0f;
            Color c = fadeImage.color;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                c.a = Mathf.Clamp01(elapsed / fadeDuration);
                fadeImage.color = c;
                yield return null;
            }
        }

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);

            if (finalScoreText != null && GameManager.Instance != null)
            {
                finalScoreText.text = "Score Final : " + GameManager.Instance.Score;
            }
        }

        // Fige le jeu pour marquer la pause de fin
        Time.timeScale = 0f;
    }

    // --- Méthode à lier au bouton Restart de l'UI ---
    public void RestartGame()
    {
        // Remet le temps normal (car il avait été figé à 0 lors de la victoire)
        Time.timeScale = 1f;

        // Réinitialise le score si le GameManager existe
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetScore();
        }

        // Recharge la scène actuelle, ce qui va relancer tous les Start() (y compris la génération de la grille et des ennemis)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}