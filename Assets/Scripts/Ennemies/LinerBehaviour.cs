using UnityEngine;

public class LinerBehaviour : Enemy
{

    // Cette classe concerne les ennemies se déplaçant en lignes

    [SerializeField] private float reachThreshold = 0.1f;
    private int currentTargetIndex = 0;


    void Start()
    {
        // Sécurité : si les targets ne sont pas encore assignés au démarrage, on évite le crash
        if (targets == null || targets.Length == 0)
        {
            // On essaie de récupérer automatiquement les plateformes proches si le LevelManager est présent
            if (LevelManager.Instance != null)
            {
                // Demande au LevelManager de lui assigner des voisins immédiatement
                LevelManager.Instance.AssignNeighborsToExistingEnemies();
            }

            // Si toujours aucun target, on quitte proprement sans désactiver bêtement le script
            if (targets == null || targets.Length == 0)
            {
                return; 
            }
        }

        // --- Le reste de ton code d'initialisation habituel (recherche du plus proche, etc.) ---
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
    }

    public override void HandleMove()
    {
        if (targets == null || targets.Length == 0) return;

        if (isJumping) return; // attend la fin du saut

        // Choisit un target parmi les voisins qui est devant ou sur les cotés
        int next = SelectNextTargetIndex();
        if (next >= 0 && next < targets.Length)
        {
            Transform t = targets[next];
            currentTargetIndex = next;
            StartJumpTowards(t);
        }
    }

    private int SelectNextTargetIndex()
    {
        if (targets == null || targets.Length == 0) return -1;

        // Filtre les targets dans un cône ±90° autour de la forward du transform
        float bestDist = float.MaxValue;
        int bestIndex = -1;
        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] == null) continue;
            if (targets[i].position == transform.position) continue;
            Vector3 dirTo = (targets[i].position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirTo);
            if (angle <= 100f) // inclut devant et côtés
            {
                float d = Vector3.Distance(transform.position, targets[i].position);
                if (d < bestDist && d > 0.01f)
                {
                    bestDist = d;
                    bestIndex = i;
                }
            }
        }

        // fallback: prend le plus proche si aucun trouvé
        if (bestIndex == -1)
        {
            float minD = float.MaxValue;
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] == null) continue;
                float d = Vector3.Distance(transform.position, targets[i].position);
                if (d < minD && d > 0.01f)
                {
                    minD = d;
                    bestIndex = i;
                }
            }
        }

        return bestIndex;
    }

    private void StartJumpTowards(Transform t)
    {
        if (t == null) return;
        // Utilise la méthode de base pour démarrer le saut avec paramètres locaux
        StartJumpTo(t, jumpHeight, jumpDuration);
    }

}
