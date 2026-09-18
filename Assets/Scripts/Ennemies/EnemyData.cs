using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyConfig", menuName = "Enemy/EnemyConfiguration", order = 1)]
public class EnemyData : ScriptableObject
{
    [System.Serializable]
    public class EnemyDefinition
    {
        [Tooltip("Unique identifier for this enemy type")]
        public string id;

        [Tooltip("Prefab used to spawn this enemy")]
        public GameObject prefab;

        [Tooltip("Maximum health for this enemy type")]
        public float maxHealth = 100f;

        [Tooltip("Movement speed (units/s)")]
        public float speed = 5f;

        [Tooltip("Damage dealt to the player on collision")]
        public int damage = 3;


    }

    [SerializeField]
    private List<EnemyDefinition> enemies = new List<EnemyDefinition>();

    public IReadOnlyList<EnemyDefinition> Enemies => enemies;

    public EnemyDefinition GetById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        return enemies.Find(e => e.id == id);
    }

    public EnemyDefinition GetAt(int index)
    {
        if (index < 0 || index >= enemies.Count) return null;
        return enemies[index];
    }

    public EnemyDefinition GetRandom()
    {
        if (enemies == null || enemies.Count == 0) return null;
        return enemies[Random.Range(0, enemies.Count)];
    }

    // Validation helper (editor-time) - keeps IDs unique if desired
    public bool HasValidEntries()
    {
        return enemies != null && enemies.Count > 0;
    }
}
