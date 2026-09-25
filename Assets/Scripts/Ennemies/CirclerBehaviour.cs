using UnityEngine;

public class CirclerBehaviour : Enemy
{


    [SerializeField] private float orbitRadius = 2f;
    [SerializeField] private float angularSpeedDeg = 90f; // degrés par seconde
    [SerializeField] private float orbitDuration = 2f; // secondes par target
    [SerializeField] private float reachThreshold = 0.15f; // distance pour considérer qu'on est sur le cercle
    [SerializeField] private float transitionSpeed = 3f; // vitesse pour rejoindre la position d'orbite

    private int currentTargetIndex = 0;
    private bool isOrbiting = false;
    private float orbitTimer = 0f;
    private float currentAngleDeg = 0f; // angle courant autour du target



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

        // initialise l'angle selon la position courante relative au target
        Vector3 toPos = transform.position - targets[currentTargetIndex].position;
        if (toPos.sqrMagnitude > 0.0001f)
        {
            currentAngleDeg = Mathf.Atan2(toPos.z, toPos.x) * Mathf.Rad2Deg;
        }
    }

    public override void HandleMove()
    {
        if (targets == null || targets.Length == 0) return;

        if (isJumping) return;

        // Circler choisit le prochain target de la liste et saute dessus en cycle
        int next = (currentTargetIndex + 1) % targets.Length;
        Transform t = targets[next];
        if (t != null)
        {
            currentTargetIndex = next;
            // démarre le saut via la méthode de base avec paramètres locaux
            StartJumpTo(t, jumpHeight, jumpDuration);
        }
    }

}

