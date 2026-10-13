using System;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    // Cette classe concerne les ennemies en générales et contient leurs fonctionnement globaux

    [SerializeField] protected Transform[] targets;
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private float destroyDelay = 0.5f;
    private bool isDead = false;

    [Header("Jumping")]
    [SerializeField] protected float jumpHeight = 1.2f;
    [SerializeField] protected float jumpDuration = 0.6f;
    protected bool isJumping = false;

    protected float health = 100f;

    protected int damage = 3;
    protected float speed = 5f;


    public float Speed { get { return speed; } set { speed = value; } }

    // Events pour Observer / Listener
    public event Action<float> OnHealthChanged;
    public event Action OnDeath ;

    public float Health
    {
        get { return health; }
        set
        {
            if (Mathf.Approximately(health, value)) return;
            health = value;
            OnHealthChanged?.Invoke(health);
            if (health <= 0f)
            {
                OnDeath?.Invoke();
                Die();
            }
        }
    }

    // Méthode utilitaire pour infliger des dégâts
    public void TakeDamage(float amount)
    {
        Health = Mathf.Max(0f, Health - amount);
    }

    // Initialise l'ennemi avec des valeurs provenant d'une définition
    public virtual void Initialize(float maxHealth, float moveSpeed, int dmg)
    {
        health = maxHealth;
        speed = moveSpeed;
        damage = dmg;
        OnHealthChanged?.Invoke(health);
    }

    // Permet d'assigner des targets (plateformes voisines)
    public void SetTargets(Transform[] t)
    {
        targets = t;
    }



    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log("Died");

        // Jouer un effet si fourni
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(50); // 50 points pour un ennemi tué !
        }

        // Désactiver les colliders pour éviter interactions post-mortem
        foreach (var col in GetComponents<Collider>())
        {
            col.enabled = false;
        }

        // Détruire l'objet après un court délai
        Destroy(gameObject, destroyDelay);

        
    }
    /// </summary>
    public void StartJumpTo(Transform target)
    {
        if (target == null || isJumping) return;
        StartCoroutine(JumpToCoroutine(target, jumpHeight, jumpDuration));
    }

    public void StartJumpTo(Transform target, float height, float duration)
    {
        if (target == null || isJumping) return;
        StartCoroutine(JumpToCoroutine(target, height, duration));
    }

    protected System.Collections.IEnumerator JumpToCoroutine(Transform target, float height, float duration)
    {
        isJumping = true;
        Vector3 start = transform.position;
        Vector3 end = target.position;
        float elapsed = 0f;

        // Face la direction du saut
        Vector3 lookDir = (end - start).normalized;
        if (lookDir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Position linéaire entre start et end
            Vector3 pos = Vector3.Lerp(start, end, t);
            // Ajout d'une parabole
            float parab = 4f * height * t * (1f - t); // peak at t=0.5
            pos += Vector3.up * parab;
            transform.position = pos;
            yield return null;
        }

        // Assure la position finale exacte
        transform.position = end;
        isJumping = false;
    }




    // Update is called once per frame
    void Update()
    {
        HandleMove();
    }

    // Fonction de mouvement différente selon le type d'ennemie, à implémenter dans les classes filles
    public abstract void HandleMove();

    public bool HasTargetsAssigned()
    {
        return targets != null && targets.Length > 0;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerBehaviour player = collision.gameObject.GetComponent<PlayerBehaviour>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
        }
    }



}
