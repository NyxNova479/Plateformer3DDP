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
        // 1. Vérification de sécurité : s'il n'y a pas de cibles valides, on demande au LevelManager
        bool hasValidTargets = false;
        if (targets != null && targets.Length > 0)
        {
            foreach (var t in targets)
            {
                if (t != null) { hasValidTargets = true; break; }
            }
        }

        if (!hasValidTargets && LevelManager.Instance != null)
        {
            LevelManager.Instance.AssignNeighborsToExistingEnemies();
        }

        // 2. Si le tableau est toujours vide, on arrête pour éviter un crash
        if (targets == null || targets.Length == 0) return;

        // 3. Trouve la cible la plus proche pour démarrer proprement
        float minDist = float.MaxValue;
        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] == null) continue;
            float d = Vector3.Distance(transform.position, targets[i].position);
            if (d < minDist)
            {
                minDist = d;
                currentTargetIndex = i;
            }
        }

        // 4. Initialisation du délai de saut
        jumpTimer = jumpDelay;
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