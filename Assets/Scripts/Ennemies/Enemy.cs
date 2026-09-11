using System;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    // Cette classe concerne les ennemies en générales et contient leurs fonctionnement globaux

    [SerializeField] protected Transform[] targets;

    protected float health;
    protected float speed = 5f;


    public float Speed {  get { return speed; } set { speed = value; } }



    // Update is called once per frame
    void Update()
    {
        HandleMove();
    }

    // Fonction de mouvement différente selon le type d'ennemie, à implémenter dans les classes filles
    public abstract void HandleMove();
    
        
    
}
