using UnityEngine;

public class LinerBehaviour : Enemy
{

    // Cette classe concerne les ennemies se déplaçant en lignes

    [SerializeField] private float reachThreshold = 0.1f;
    private int currentTargetIndex = 0;

    void Start()
    {
        if (targets == null || targets.Length == 0)
        {
            Debug.LogWarning("LinerBehaviour: no targets assigned.");
            enabled = false;
            return;
        }

        // Positionne l'index du target initial sur le plus proche
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
    }

    public override void HandleMove()
    {
        if (targets == null || targets.Length == 0) return;

        Transform target = targets[currentTargetIndex];

        // Déplacement constant vers la cible courante
        float step = speed * Time.deltaTime;
        float distance = Vector3.Distance(transform.position, target.position);
        transform.position = Vector3.MoveTowards(transform.position, target.position, step);

        // Si on a atteint (ou suffisamment approché) la cible, passe à la suivante
        if (distance <= Mathf.Max(reachThreshold, step))
        {
            currentTargetIndex = (currentTargetIndex + 1) % targets.Length;
        }
    }

}
