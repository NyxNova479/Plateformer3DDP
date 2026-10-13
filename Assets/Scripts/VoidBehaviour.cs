using UnityEngine;

public class VoidBehaviour : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. Réinitialise le score
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResetScore();
            }

            // 2. Remet le joueur au point de départ
            other.transform.position = new Vector3(3f, 1f, 5f);

            // Stoppe la vélocité pour éviter qu'il ne conserve son élan de chute
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
            }

            // 3. Réinitialise toutes les plateformes du niveau (visites et états temporaires)
            Platform[] allPlatforms = FindObjectsByType<Platform>();
            foreach (var plat in allPlatforms)
            {
                plat.ResetPlatformState();
            }

            Debug.Log("Le joueur est tombé dans le vide : Score et plateformes réinitialisés.");
        }
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<EnemySafetyNet>().RespawnEnemy();
        }
    }
}