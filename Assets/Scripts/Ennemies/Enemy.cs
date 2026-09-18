using System;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    // Cette classe concerne les ennemies en générales et contient leurs fonctionnement globaux

    [SerializeField] protected Transform[] targets;

    protected float health = 100f;

    protected int damage = 3;
    protected float speed = 5f;


    public float Speed { get { return speed; } set { speed = value; } }

    // Events pour Observer / Listener
    public event Action<float> OnHealthChanged;
    public event Action OnDeath;

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
            }
        }
    }

    // Méthode utilitaire pour infliger des dégâts
    public void TakeDamage(float amount)
    {
        Health = Mathf.Max(0f, Health - amount);
    }



    // Update is called once per frame
    void Update()
    {
        HandleMove();
    }

    // Fonction de mouvement différente selon le type d'ennemie, à implémenter dans les classes filles
    public abstract void HandleMove();

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerBehaviour player = collision.gameObject.GetComponent<PlayerBehaviour>();
            if (player != null)
            {
                player.Health -= damage;
            }
        }
    }



}
