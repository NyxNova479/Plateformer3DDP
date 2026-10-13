using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private int score = 0;
    public int Score => score;

    public event Action<int> OnScoreChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AddScore(int amount)
    {
        score += amount;
        OnScoreChanged?.Invoke(score);
        Debug.Log("Score actuel : " + score);
    }
    public void ResetScore()
    {
        score += 0;
        OnScoreChanged?.Invoke(score);
        Debug.Log("Score actuel : " + score);
    }

    public bool SpendScore(int cost)
    {
        if (score >= cost)
        {
            score -= cost;
            OnScoreChanged?.Invoke(score);
            return true;
        }
        return false; // Pas assez de points
    }
}