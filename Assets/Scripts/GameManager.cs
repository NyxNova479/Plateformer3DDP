using UnityEngine;
using TMPro;
using UnityEditor;

public class GameManager : MonoBehaviour
{

    private static GameManager instance;

    public GameManager Instance() => instance;

    private void Awake()
    {
        if(instance != null) Destroy(this.gameObject);
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
