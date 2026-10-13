using UnityEngine;
using UnityEngine.UI;
using TMPro; // Utilise TextMeshPro si tu en as besoin, ou remplace par UnityEngine.UI.Text

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText; // Ou Text classique
    [SerializeField] private GameObject powerUpPanel;   // Panel affichant les choix de power-ups

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
        if (powerUpPanel != null) powerUpPanel.SetActive(false); // Cache le menu au départ
    }

    private void OnEnable()
    {
        // S'abonne aux changements de score du GameManager (Observer Pattern)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged += UpdateScoreUI;
            UpdateScoreUI(GameManager.Instance.Score);
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged -= UpdateScoreUI;
        }
    }

    private void UpdateScoreUI(int newScore)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score : " + newScore;
        }
    }

    // Ouvre le choix des Power-Ups (appelé par la boutique)
    public void OpenPowerUpShopMenu()
    {
        if (powerUpPanel != null)
        {
            powerUpPanel.SetActive(true);
            Time.timeScale = 0f; // Pause le jeu le temps du choix
        }
    }

    // Bouton 1 : Choisir le boost de Vitesse
    public void BuySpeedBoost()
    {
        if (GameManager.Instance != null && GameManager.Instance.SpendScore(100))
        {
            PlayerBehaviour player = FindAnyObjectByType<PlayerBehaviour>();
            if (player != null) player.Speed += 4f;
            CloseShopMenu();
        }
    }

    // Bouton 2 : Choisir le boost de Saut
    public void BuyJumpBoost()
    {
        if (GameManager.Instance != null && GameManager.Instance.SpendScore(100))
        {
            // Tu peux ajouter une propriété jumpForce publique dans PlayerBehaviour si besoin
            CloseShopMenu();
        }
    }

    public void CloseShopMenu()
    {
        if (powerUpPanel != null) powerUpPanel.SetActive(false);
        Time.timeScale = 1f; // Reprend le jeu
    }
}