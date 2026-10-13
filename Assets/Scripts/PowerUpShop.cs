using UnityEngine;

public class PowerUpShop : MonoBehaviour
{
    [SerializeField] private int powerUpCost = 100;
    [SerializeField] private float speedBoost = 5f;
    [SerializeField] private float jumpBoost = 3f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerBehaviour player = other.GetComponent<PlayerBehaviour>();
            if (player != null && GameManager.Instance != null)
            {
                if (GameManager.Instance.SpendScore(powerUpCost))
                {
                    // Applique le Power-Up pour affronter le boss !
                    player.Speed += speedBoost;
                    Debug.Log("Power-Up acheté ! Le joueur est plus rapide et puissant pour le Boss.");
                    Destroy(gameObject); // Désactive la boutique après achat
                }
                else
                {
                    Debug.Log("Pas assez de points ! Il vous faut " + powerUpCost + " points.");
                }
            }
        }
    }
}