using UnityEngine;

public class CirclerBehaviour : Enemy
{
    [Header("Jump Settings")]
    [SerializeField] private float jumpDelay = 2f; // Temps d'attente entre chaque saut
    [SerializeField] private float reachThreshold = 0.15f;

    private int currentTargetIndex = 0;
    private float jumpTimer = 0f;

    void Start()
    {
        if (targets == null || targets.Length == 0)
        {
            Debug.LogWarning("CirclerBehaviour: no targets assigned.");
            enabled = false;
            return;
        }

        // Choisit le target le plus proche au démarrage
        float minDist = float.MaxValue;
        for (int i = 0; i < targets.Length; i++)
        {
            float d = Vector3.Distance(transform.position, targets[i].position);
            if (d < minDist)
            {
                minDist = d;
                currentTargetIndex = i;
            }
        }

        jumpTimer = jumpDelay; // Prêt à sauter rapidement au début
    }

    public override void HandleMove()
    {
        if (targets == null || targets.Length == 0) return;


        if (isJumping) return;

        // Gestion du délai d'attente entre les sauts
        jumpTimer += Time.deltaTime;
        if (jumpTimer < jumpDelay) return;

        // Le délai est écoulé, on choisit la prochaine cible en cycle
        int next = (currentTargetIndex + 1) % targets.Length;
        Transform t = targets[next];

        if (t != null)
        {
            currentTargetIndex = next;
            jumpTimer = 0f; // Remet le timer à zéro pour attendre le prochain saut

            // Lance le saut parabolique vers la plateforme cible
            StartJumpTo(t, jumpHeight, jumpDuration);
        }
    }


}