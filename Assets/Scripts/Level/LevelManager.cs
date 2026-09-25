using UnityEngine;


public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Enemy Data")]
    [SerializeField] private EnemyData enemyData;
    [Header("Factories")]
    [SerializeField] private EnemyFactory enemyFactory;
    [SerializeField] private PlatformFactory platformFactory;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Level Bounds")]
    [SerializeField] private Vector3 boundsCenter = Vector3.zero;
    [SerializeField] private Vector3 boundsSize = new Vector3(50f, 10f, 50f);

    [Header("Platform Graph")]
    [SerializeField] private float platformNeighborRadius = 5f;

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

    private Platform[] platforms;
    private System.Collections.Generic.Dictionary<Platform, Transform[]> platformNeighbors = new System.Collections.Generic.Dictionary<Platform, Transform[]>();

    private void Start()
    {
        BuildPlatformGraph();
        // Optionally assign neighbors to existing enemies in scene
        AssignNeighborsToExistingEnemies();
    }

    /// <summary>
    /// Construit la liste de voisins pour chaque plateforme basée sur la distance.
    /// </summary>
    public void BuildPlatformGraph()
    {
        platforms = FindObjectsOfType<Platform>();
        platformNeighbors.Clear();
        if (platforms == null || platforms.Length == 0) return;

        for (int i = 0; i < platforms.Length; i++)
        {
            var p = platforms[i];
            var neighbors = new System.Collections.Generic.List<Transform>();
            for (int j = 0; j < platforms.Length; j++)
            {
                if (i == j) continue;
                var other = platforms[j];
                if (Vector3.Distance(p.transform.position, other.transform.position) <= platformNeighborRadius)
                {
                    neighbors.Add(other.transform);
                }
            }
            platformNeighbors[p] = neighbors.ToArray();
        }
    }

    /// <summary>
    /// Retourne les voisins (Transforms) d'une plateforme donnée si connus.
    /// </summary>
    public Transform[] GetNeighborsForPlatform(Platform p)
    {
        if (p == null) return null;
        if (platformNeighbors != null && platformNeighbors.TryGetValue(p, out var list)) return list;
        return null;
    }

    /// <summary>
    /// Assigne aux ennemis présents dans la scène des targets basés sur la plateforme la plus proche.
    /// </summary>
    public void AssignNeighborsToExistingEnemies()
    {
        var enemies = FindObjectsOfType<Enemy>();
        if (enemies == null || enemies.Length == 0) return;
        if (platforms == null || platforms.Length == 0) BuildPlatformGraph();

        foreach (var e in enemies)
        {
            Platform nearest = FindNearestPlatform(e.transform.position);
            if (nearest != null)
            {
                var neigh = GetNeighborsForPlatform(nearest);
                e.SetTargets(neigh);
            }
        }
    }

    private Platform FindNearestPlatform(Vector3 pos)
    {
        Platform best = null;
        float bestDist = float.MaxValue;
        if (platforms == null || platforms.Length == 0) return null;
        foreach (var p in platforms)
        {
            float d = Vector3.Distance(pos, p.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = p;
            }
        }
        return best;
    }


    public bool IsInsideBounds(Vector3 position)
    {
        return levelBounds.Contains(position);
    }

    public Enemy SpawnEnemyById(string id, int spawnIndex = -1)
    {
        if (enemyFactory != null)
        {
            return enemyFactory.SpawnEnemyById(id, spawnIndex);
        }

        // Fallback: comportement précédent si aucune factory assignée
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
            // assigner targets en fonction de la plateforme la plus proche du spawn
            if (platforms == null || platforms.Length == 0) BuildPlatformGraph();
            var nearest = FindNearestPlatform(pos);
            if (nearest != null)
            {
                var neigh = GetNeighborsForPlatform(nearest);
                enemy.SetTargets(neigh);
            }
        }
        return enemy;
    }


    public Enemy SpawnEnemyPrefab(GameObject prefab, Vector3 position)
    {
        if (enemyFactory != null)
        {
            return enemyFactory.SpawnEnemyPrefab(prefab, position);
        }

        if (prefab == null) return null;
        if (!IsInsideBounds(position))
        {
            Debug.LogWarning("Spawn position outside bounds, aborting spawn.");
            return null;
        }

        GameObject go = Instantiate(prefab, position, Quaternion.identity);
        return go.GetComponent<Enemy>();
    }

    /// <summary>
    /// Retourne une position de spawn selon l'index (utilisé par les factories)
    /// </summary>
    public Vector3 GetSpawnPosition(int spawnIndex)
    {
        return ChooseSpawnPosition(spawnIndex);
    }

    /// <summary>
    /// Donne les voisins de la plateforme la plus proche d'une position donnée.
    /// </summary>
    public Transform[] GetNeighborsForPosition(Vector3 position)
    {
        if (platforms == null || platforms.Length == 0) BuildPlatformGraph();
        var nearest = FindNearestPlatform(position);
        if (nearest == null) return null;
        return GetNeighborsForPlatform(nearest);
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
        // Dessine les plateformes et leurs liens de voisinage si disponibles
        if (platforms != null && platforms.Length > 0)
        {
            Gizmos.color = Color.cyan;
            foreach (var p in platforms)
            {
                if (p == null) continue;
                Gizmos.DrawWireSphere(p.transform.position, 0.15f);
                if (platformNeighbors != null && platformNeighbors.TryGetValue(p, out var neigh))
                {
                    Gizmos.color = Color.magenta;
                    foreach (var n in neigh)
                    {
                        if (n == null) continue;
                        Gizmos.DrawLine(p.transform.position, n.position);
                    }
                }
            }
        }
    }

#endif
}
        


