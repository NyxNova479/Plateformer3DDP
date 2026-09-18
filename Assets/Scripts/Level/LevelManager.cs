using UnityEngine;


public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Enemy Data")]
    [SerializeField] private EnemyData enemyData;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Level Bounds")]
    [SerializeField] private Vector3 boundsCenter = Vector3.zero;
    [SerializeField] private Vector3 boundsSize = new Vector3(50f, 10f, 50f);

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;

    private Bounds levelBounds => new Bounds(boundsCenter, boundsSize);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    public bool IsInsideBounds(Vector3 position)
    {
        return levelBounds.Contains(position);
    }

    public Enemy SpawnEnemyById(string id, int spawnIndex = -1)
    {
        if (enemyData == null)
        {
            Debug.LogWarning("LevelManager: enemyData not assigned.");
            return null;
        }

        var def = enemyData.GetById(id);
        if (def == null || def.prefab == null)
        {
            Debug.LogWarning($"LevelManager: definition '{id}' not found or prefab null.");
            return null;
        }

        Vector3 pos = ChooseSpawnPosition(spawnIndex);
        if (!IsInsideBounds(pos))
        {
            Debug.LogWarning("Spawn position outside bounds, aborting spawn.");
            return null;
        }

        GameObject go = Instantiate(def.prefab, pos, Quaternion.identity);
        Enemy enemy = go.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.Initialize(def.maxHealth, def.speed, def.damage);
        }
        return enemy;
    }


    public Enemy SpawnEnemyPrefab(GameObject prefab, Vector3 position)
    {
        if (prefab == null) return null;
        if (!IsInsideBounds(position))
        {
            Debug.LogWarning("Spawn position outside bounds, aborting spawn.");
            return null;
        }

        GameObject go = Instantiate(prefab, position, Quaternion.identity);
        return go.GetComponent<Enemy>();
    }

    private Vector3 ChooseSpawnPosition(int spawnIndex)
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int idx = spawnIndex;
            if (spawnIndex < 0 || spawnIndex >= spawnPoints.Length)
                idx = Random.Range(0, spawnPoints.Length);
            return spawnPoints[idx].position;
        }

        // fallback : centre des bounds
        return levelBounds.center;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;
        Gizmos.color = new Color(0f, 1f, 0f, 0.15f);
        Gizmos.DrawCube(boundsCenter, boundsSize);

        if (spawnPoints != null)
        {
            Gizmos.color = Color.yellow;
            foreach (var s in spawnPoints)
            {
                if (s == null) continue;
                Gizmos.DrawSphere(s.position, 0.25f);

            }
        }
    }

#endif
}

