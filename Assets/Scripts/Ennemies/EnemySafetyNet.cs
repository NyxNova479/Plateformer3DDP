using UnityEngine;

public class EnemySafetyNet : MonoBehaviour
{
    private Vector3 initialSpawnPosition;

    void Start()
    {
        initialSpawnPosition = transform.position;
    }



    public void RespawnEnemy()
    {
        // Remet l'ennemi à sa position de spawn initiale et coupe sa vélocité
        transform.position = initialSpawnPosition;
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}