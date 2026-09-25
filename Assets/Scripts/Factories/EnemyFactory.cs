using UnityEngine;

[CreateAssetMenu(fileName = "EnemyFactory", menuName = "Factory/EnemyFactory", order = 1)]
public class EnemyFactory : ScriptableObject
{
    [Header("Data")]
    [SerializeField] private EnemyData enemyData;

    [Header("Spawn options")]
    [SerializeField] private Transform[] spawnPoints;

    /// <summary>
    /// Spawns an enemy by definition id using LevelManager for bounds and neighbors.
    /// </summary>
    public Enemy SpawnEnemyById(string id, int spawnIndex = -1)
    {
        if (enemyData == null)
        {
            Debug.LogWarning("EnemyFactory: enemyData not assigned.");
            return null;
        }

        var def = enemyData.GetById(id);
        if (def == null || def.prefab == null)
        {
            Debug.LogWarning($"EnemyFactory: definition '{id}' not found or prefab null.");
            return null;
        }

        Vector3 pos;
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int idx = spawnIndex;
            if (spawnIndex < 0 || spawnIndex >= spawnPoints.Length) idx = Random.Range(0, spawnPoints.Length);
            pos = spawnPoints[idx].position;
        }
        else if (LevelManager.Instance != null)
        {
            pos = LevelManager.Instance.GetSpawnPosition(spawnIndex);
        }
        else
        {
            pos = Vector3.zero;
        }

        if (LevelManager.Instance != null && !LevelManager.Instance.IsInsideBounds(pos))
        {
            Debug.LogWarning("EnemyFactory: spawn position outside bounds, aborting spawn.");
            return null;
        }

        GameObject go = Instantiate(def.prefab, pos, Quaternion.identity);
        Enemy enemy = go.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.Initialize(def.maxHealth, def.speed, def.damage);
            // assign neighbors from LevelManager if available
            if (LevelManager.Instance != null)
            {
                var neigh = LevelManager.Instance.GetNeighborsForPosition(pos);
                enemy.SetTargets(neigh);
            }
        }
        return enemy;
    }

    public Enemy SpawnEnemyPrefab(GameObject prefab, Vector3 position)
    {
        if (prefab == null) return null;
        if (LevelManager.Instance != null && !LevelManager.Instance.IsInsideBounds(position))
        {
            Debug.LogWarning("EnemyFactory: spawn position outside bounds, aborting spawn.");
            return null;
        }

        GameObject go = Instantiate(prefab, position, Quaternion.identity);
        return go.GetComponent<Enemy>();
    }
}
