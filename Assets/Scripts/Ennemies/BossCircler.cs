using UnityEngine;
using System.Collections;

public class BossCircler : Enemy
{
    [Header("Boss Settings")]
    [SerializeField] private float orbitRadius = 4f;       // Plus grand rayon que les ennemis normaux
    [SerializeField] private float angularSpeedDeg = 60f;  // Vitesse d'orbite
    [SerializeField] private float attackDelay = 2.5f;     // Le gros délai entre les attaques
    [SerializeField] private GameObject projectilePrefab;  // Un projectile à lancer (optionnel)
    [SerializeField] private Transform firePoint;          // Point d'origine du tir

    private int currentTargetIndex = 0;
    private float attackTimer = 0f;
    private bool isEnraged = false; // Phase 2 quand il a moins de 50% de vie

    void Start()
    {
        // Initialisation standard du boss (plus de vie par défaut)
        health = 300f;
        speed = 4f;
        damage = 20;

        if (targets == null || targets.Length == 0)
        {
            Debug.LogWarning("BossCircler: Aucun target assigné !");
            enabled = false;
        }
    }

    public override void HandleMove()
    {
        if (targets == null || targets.Length == 0) return;

        Transform target = targets[currentTargetIndex];

        // Gestion de la Phase 2 (Enragé si PV < 50%)
        if (!isEnraged && health <= 150f)
        {
            isEnraged = true;
            angularSpeedDeg *= 1.5f; // Il tourne plus vite
            attackDelay *= 0.6f;     // Il attaque plus souvent
            Debug.Log("LE BOSS ENTRE EN PHASE 2 !");
        }

        // Mouvement d'orbite autour de la cible actuelle
        float angleRad = Time.time * (angularSpeedDeg * Mathf.Deg2Rad);
        Vector3 orbitOffset = new Vector3(Mathf.Cos(angleRad), 0f, Mathf.Sin(angleRad)) * orbitRadius;
        Vector3 desiredPos = target.position + orbitOffset;

        transform.position = Vector3.MoveTowards(transform.position, desiredPos, speed * Time.deltaTime);
        transform.LookAt(target.position + Vector3.up * 0.5f);

        // Gestion du délai d'attaque (le "gros délai" que tu voulais)
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackDelay)
        {
            attackTimer = 0f;
            PerformBossAttack();
        }
    }

    private void PerformBossAttack()
    {
        // Option 1 : Attaque à distance (Tir de projectile vers le joueur)
        if (projectilePrefab != null && firePoint != null)
        {
            Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            Debug.Log("Boss tire un projectile !");
        }
        else
        {
            // Option 2 : Onde de choc / Attaque de zone autour du boss
            Debug.Log("Boss déclenche une onde de choc à retardement !");
            StartCoroutine(ShockwaveAttack());
        }
    }

    private IEnumerator ShockwaveAttack()
    {
        // Petit effet visuel ou temps de chargement (le gros délai)
        yield return new WaitForSeconds(0.5f);

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
        // Le boss rapporte un énorme score pour récompenser la fin du jeu
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(500);
        }

        Debug.Log("Boss vaincu ! Niveau terminé !");
        base.Die();

        // Ici, tu pourrais déclencher l'affichage de l'écran de Victoire / Fin de jeu
    }
}