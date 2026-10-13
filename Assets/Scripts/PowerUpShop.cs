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
            UIManager.Instance.OpenPowerUpShopMenu();
        }
    }
}