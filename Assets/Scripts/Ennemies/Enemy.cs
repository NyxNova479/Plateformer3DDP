using System;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    // Cette classe concerne les ennemies en générales et contient leurs fonctionnement globaux

    [SerializeField] protected Transform[] targets;

    protected float health = 100f;

    protected int damage = 3;
    protected float speed = 5f;


    public float Speed {  get { return speed; } set { speed = value; } }

    public float Health { get { return health; } set { health = value; } }



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
