using UnityEngine;

public class MovingPlatformRider : MonoBehaviour
{
    private Transform currentPlatform;
    private Collider platformCollider;
    private Vector3 lastPlatformPos;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (currentPlatform != null)
        {
            // Sécurité 1 : Si la plateforme est détruite ou désactivée
            if (currentPlatform == null || !currentPlatform.gameObject.activeInHierarchy)
            {
                SetPlatform(null);
                return;
            }

            // Sécurité 2 : Si le collider de la plateforme est désactivé (ex: plateforme temporaire qui tombe)
            if (platformCollider != null && !platformCollider.enabled)
            {
                SetPlatform(null);
                return;
            }

            // Suit le mouvement de la plateforme
            Vector3 platformDelta = currentPlatform.position - lastPlatformPos;

            if (rb != null)
            {
                rb.MovePosition(rb.position + platformDelta);
            }
            else
            {
                transform.position += platformDelta;
            }

            lastPlatformPos = currentPlatform.position;
        }
    }

    public void SetPlatform(Transform plat)
    {
        currentPlatform = plat;
        if (plat != null)
        {
            lastPlatformPos = plat.position;
            // Récupère le collider de la plateforme pour surveiller son état
            platformCollider = plat.GetComponent<Collider>();
            if (platformCollider == null)
            {
                platformCollider = plat.GetComponentInChildren<Collider>();
            }
        }
        else
        {
            platformCollider = null;
        }
    }
}