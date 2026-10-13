using UnityEngine;
using System.Collections;

public class BossCircler : Enemy
{
    [Header("Boss Jump & Combat Settings")]
    [SerializeField] private float jumpDelay = 2.5f;     // Le gros délai d'attente entre chaque saut
    [SerializeField] private float attackDelay = 4f;     // Délai entre chaque grosse attaque spéciale
    [SerializeField] private GameObject projectilePrefab;  // Optionnel : projectile
    [SerializeField] private Transform firePoint;          // Point d'origine du tir

    private int currentTargetIndex = 0;
    private float jumpTimer = 0f;
    private float attackTimer = 0f;
    private bool isEnraged = false; // Phase 2 quand PV <= 50%

    void Start()
    {
        health = 300f;
        speed = 5f;
        damage = 20;

        if (targets == null || targets.Length == 0)
        {
            Debug.LogWarning("BossCircler: Aucun target assigné !");
            enabled = false;
            return;
        }

        // Trouve la plateforme cible la plus proche au départ
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

        jumpTimer = jumpDelay; // Prêt à bouger rapidement au lancement
    }

    public override void HandleMove()
    {
        if (targets == null || targets.Length == 0) return;

        // Gestion de la Phase 2 (Enragé si PV <= 50%)
        if (!isEnraged && health <= 150f)
        {
            isEnraged = true;
            jumpDelay *= 0.6f;     // Il saute plus souvent / plus vite
            attackDelay *= 0.6f;   // Il attaque plus souvent
            Debug.Log("LE BOSS ENTRE EN PHASE 2 (Vitesse accrue) !");
        }

        // 1. Gestion des attaques périodiques (indépendantes des sauts)
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackDelay)
        {
            attackTimer = 0f;
            PerformBossAttack();
        }

        // 2. Si le boss est déjà en train de sauter en l'air, on attend qu'il atterrisse
        if (isJumping) return;

        // 3. Gestion du délai d'attente entre les sauts
        jumpTimer += Time.deltaTime;
        if (jumpTimer < jumpDelay) return;

        // 4. Le délai est écoulé : on choisit la prochaine cible en cycle et on saute !
        int nextTargetIndex = (currentTargetIndex + 1) % targets.Length;
        Transform targetPlatform = targets[nextTargetIndex];

        if (targetPlatform != null)
        {
            currentTargetIndex = nextTargetIndex;
            jumpTimer = 0f; // Remet le timer du saut à zéro

            // Lance le saut parabolique vers la plateforme cible
            StartJumpTo(targetPlatform, jumpHeight, jumpDuration);
        }
    }

    private void PerformBossAttack()
    {
        if (projectilePrefab != null && firePoint != null)
        {
            // Vérifie si le joueur est dans un rayon de détection (ex: 15 unités)
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null && Vector3.Distance(transform.position, playerObj.transform.position) <= 15f)
            {
                // Oriente le point de tir vers le joueur avant d'instancier
                firePoint.LookAt(playerObj.transform.position + Vector3.up * 0.5f);
            }

            Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            Debug.Log("Boss tire un projectile vers le joueur !");
        }
        else
        {
            StartCoroutine(ShockwaveAttack());
        }
    }

    private IEnumerator ShockwaveAttack()
    {
        // Petit temps de chargement de l'attaque
        yield return new WaitForSeconds(0.4f);

        Collider[] hits = Physics.OverlapSphere(transform.position, 6f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerBehaviour player = hit.GetComponent<PlayerBehaviour>();
                if (player != null)
                {
                    player.TakeDamage(damage);
                }
            }
        }
    }



    protected override void Die()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(500);
        }

        Debug.Log("Boss vaincu ! Niveau terminé !");
        base.Die();
    }
}