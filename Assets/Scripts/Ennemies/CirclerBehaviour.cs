using UnityEngine;

public class CirclerBehaviour : Enemy
{
    // Cette classe fait orbiter l'ennemi autour des targets.
    // Le comportement : se déplace vers le cercle défini autour d'un target,
    // orbite pendant une durée, puis passe au target suivant.

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

        Transform target = targets[currentTargetIndex];

        // position désirée sur le cercle autour du target
        float angleRad = currentAngleDeg * Mathf.Deg2Rad;
        Vector3 orbitOffset = new Vector3(Mathf.Cos(angleRad), 0f, Mathf.Sin(angleRad)) * orbitRadius;
        Vector3 desiredOrbitPos = target.position + orbitOffset;

        if (!isOrbiting)
        {
            // Se déplacer vers le cercle
            transform.position = Vector3.MoveTowards(transform.position, desiredOrbitPos, transitionSpeed * Time.deltaTime);

            // Si proche de la position d'orbite, commencer à orbiter
            if (Vector3.Distance(transform.position, desiredOrbitPos) <= Mathf.Max(reachThreshold, transitionSpeed * Time.deltaTime))
            {
                isOrbiting = true;
                orbitTimer = 0f;

                Vector3 toPos = transform.position - target.position;
                if (toPos.sqrMagnitude > 0.0001f)
                    currentAngleDeg = Mathf.Atan2(toPos.z, toPos.x) * Mathf.Rad2Deg;
            }
        }
        else
        {
            // Orbite autour du target
            float deltaAngle = angularSpeedDeg * Time.deltaTime;
            currentAngleDeg += deltaAngle;

            // Calcule la nouvelle position d'orbite et se déplace dessus avec un pas égal à l'arc parcouru
            float arcLength = Mathf.Deg2Rad * deltaAngle * orbitRadius;
            angleRad = currentAngleDeg * Mathf.Deg2Rad;
            orbitOffset = new Vector3(Mathf.Cos(angleRad), 0f, Mathf.Sin(angleRad)) * orbitRadius;
            desiredOrbitPos = target.position + orbitOffset;

            transform.position = Vector3.MoveTowards(transform.position, desiredOrbitPos, arcLength);

            orbitTimer += Time.deltaTime;
            if (orbitTimer >= orbitDuration)
            {
                // Fin d'orbite pour ce target -> passer au suivant
                isOrbiting = false;
                currentTargetIndex = (currentTargetIndex + 1) % targets.Length;
                // Recalcule angle initial par rapport au nouveau target
                Vector3 toPos = transform.position - targets[currentTargetIndex].position;
                if (toPos.sqrMagnitude > 0.0001f)
                    currentAngleDeg = Mathf.Atan2(toPos.z, toPos.x) * Mathf.Rad2Deg;
            }
        }
    }

}

