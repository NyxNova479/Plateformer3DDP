using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifetime = 4f;
    [SerializeField] private float damage = 15f;

    void Start()
    {
        // Détruit le projectile après un certain temps pour nettoyer la scène
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Avance tout droit
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerBehaviour player = other.GetComponent<PlayerBehaviour>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
            Destroy(gameObject); // Disparaît en touchant le joueur
        }
    }
}