using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Génère une grille de plateformes dans les bounds définis par LevelManager en utilisant PlatformFactory.
/// Optionnellement spawn des ennemis sur des plateformes via EnemyFactory et prefab.
/// </summary>
public class GridLevelGenerator : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private int rows = 5;
    [SerializeField] private int cols = 5;
    [SerializeField] private float spacingX = 3f;
    [SerializeField] private float spacingZ = 3f;
    [SerializeField] private float yOffset = 0f; // offset vertical relatif au bounds center

    [Header("Factories")]
    [SerializeField] private PlatformFactory platformFactory;
    [SerializeField] private EnemyFactory enemyFactory; // optionnel

    [Header("Platform Definition")]
    [SerializeField] private string platformDefinitionId = "";
    [SerializeField] private int platformDefinitionIndex = -1;
    [Header("Alternation")]
    [Tooltip("Si vrai, alterne les définitions listées dans platformDefinitionIndices pour chaque plateforme générée")]
    [SerializeField] private bool alternateDefinitions = false;
    [Tooltip("Indices des définitions (dans PlateformsData) à alterner en boucle")]
    [SerializeField] private int[] platformDefinitionIndices = new int[0];

    [Header("Enemies")]
    [SerializeField] private bool spawnEnemies = false;
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private int enemyCount = 5;

    [Header("Generation")]
    [SerializeField] private bool autoGenerateOnStart = true;
    [Header("Controlled Randomness")]
    [Range(0f, 1f)][SerializeField] private float movingProbability = 0.2f;
    [Tooltip("Minimum extra spacing required around a moving platform to avoid clipping (units)")]
    [SerializeField] private float minSpacingForMoving = 1.0f;
    [Tooltip("Approximate platform radius used for spacing checks")]
    [SerializeField] private float platformRadius = 0.5f;

    [Header("Connectivity")]
    [Tooltip("Maximum jump distance allowed between platforms for traversal")]
    [SerializeField] private float jumpMaxDistance = 3.5f;
    [Tooltip("Number of attempts to insert bridging platforms when graph is disconnected")]
    [SerializeField] private int maxBridgeAttempts = 10;

    private List<Platform> spawnedPlatforms = new List<Platform>();

    void Start()
    {
        if (!autoGenerateOnStart) return;
        GenerateGrid();
    }

    /// <summary>
    /// Génère la grille et reconstruit le graphe de plateformes.
    /// </summary>
    public void GenerateGrid()
    {
        spawnedPlatforms.Clear();

        if (LevelManager.Instance == null)
        {
            Debug.LogWarning("GridLevelGenerator: LevelManager.Instance not found.");
            return;
        }

        // Récupère les bounds
        Vector3 center = LevelManager.Instance != null ? LevelManager.Instance.GetSpawnPosition(0) : Vector3.zero;

        Vector3 boundsCenter = LevelManager.Instance != null ? (LevelManager.Instance.GetSpawnPosition(0)) : Vector3.zero;

        // Compute total size
        float totalWidth = (cols - 1) * spacingX;
        float totalDepth = (rows - 1) * spacingZ;

        Vector3 origin = boundsCenter - new Vector3(totalWidth * 0.5f, 0f, totalDepth * 0.5f);
        origin.y += Random.Range(0, yOffset);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Vector3 pos = origin + new Vector3(c * spacingX, 0f, r * spacingZ);
                // ensure inside bounds
                if (LevelManager.Instance != null && !LevelManager.Instance.IsInsideBounds(pos))
                    continue;

                Platform p = null;
                int defIndexToUse = platformDefinitionIndex;
                string defIdToUse = platformDefinitionId;
                if (alternateDefinitions && platformDefinitionIndices != null && platformDefinitionIndices.Length > 0)
                {
                    int idx = (r * cols + c) % platformDefinitionIndices.Length;
                    defIndexToUse = platformDefinitionIndices[idx];
                    defIdToUse = "";
                }

                if (platformFactory != null)
                {
                    p = platformFactory.SpawnPlatformAt(pos, defIdToUse, defIndexToUse);
                }
                else
                {
                    Debug.LogWarning("GridLevelGenerator: platformFactory not assigned. Cannot spawn platform.");
                }

                if (p != null) spawnedPlatforms.Add(p);
            }
        }

        // Controlled random assignment of moving platforms
        ApplyMovingFlagsControlled();

        // Ensure connectivity (insert bridging platforms if necessary)
        if (!IsGraphConnected())
        {
            TryEnsureConnectivity();
        }

        // Rebuild graph
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.BuildPlatformGraph();
            LevelManager.Instance.AssignNeighborsToExistingEnemies();
        }
        // Spawn enemies on random platforms
        if (spawnEnemies && enemyPrefabs != null && enemyFactory != null && spawnedPlatforms.Count > 0)
        {
            for (int i = 0; i < enemyCount; i++)
            {
                int idx = Random.Range(0, spawnedPlatforms.Count);
                var plat = spawnedPlatforms[idx];
                if (plat == null) continue;
                Vector3 spawnPos = plat.transform.position + Vector3.up * 0.5f;
                int rand = Random.Range(0, enemyPrefabs.Length);
                enemyFactory.SpawnEnemyPrefab(enemyPrefabs[rand], spawnPos);
            }

            // After spawning enemies, assign neighbors again
            if (LevelManager.Instance != null)
                LevelManager.Instance.AssignNeighborsToExistingEnemies();
        }
    }


    private void ApplyMovingFlagsControlled()
    {
        if (spawnedPlatforms == null || spawnedPlatforms.Count == 0) return;

        for (int i = 0; i < spawnedPlatforms.Count; i++)
        {
            var p = spawnedPlatforms[i];
            if (p == null) continue;

            if (Random.value <= movingProbability)
            {
                // Récupère l'amplitude définie sur la plateforme
                float movementRange = p.GetMovementRange();
                Vector3 moveDir = Vector3.right; // Direction par défaut

                // Calcul de l'encombrement total sur l'axe (Rayon plateforme * 2 + amplitude * 2 + marge de sécurité)
                float totalReach = movementRange + minSpacingForMoving + (platformRadius * 2f);
                bool spaceIsClear = true;

                foreach (var other in spawnedPlatforms)
                {
                    if (other == null || other == p) continue;

                    // Calcule la distance séparant les deux plateformes sur l'axe du mouvement (X)
                    float distOnAxis = Mathf.Abs(p.transform.position.x - other.transform.position.x);
                    float distOnOtherAxes = Vector3.Distance(
                        new Vector3(0, p.transform.position.y, p.transform.position.z),
                        new Vector3(0, other.transform.position.y, other.transform.position.z)
                    );

                    // Si les plateformes sont sur la même ligne/hauteur et que la trajectoire croise l'autre plateforme
                    if (distOnOtherAxes < (platformRadius * 2f) && distOnAxis < totalReach)
                    {
                        spaceIsClear = false;
                        break;
                    }
                }

                if (spaceIsClear)
                {
                    var def = new PlateformsData.PlatformDefinition
                    {
                        moves = true,
                        speed = 2f,
                        dir = moveDir,
                        isTemporary = false,
                        movementRange = movementRange,
                        disappearDelay = 0.5f,
                        respawnTime = 3f
                    };
                    p.ApplyDefinition(def);
                }
            }
        }
    }

    private bool IsGraphConnected()
    {
        if (spawnedPlatforms == null || spawnedPlatforms.Count == 0) return true;
        int n = spawnedPlatforms.Count;
        var visited = new bool[n];

        // build adjacency based on jumpMaxDistance
        System.Collections.Generic.List<int>[] adj = new System.Collections.Generic.List<int>[n];
        for (int i = 0; i < n; i++) adj[i] = new System.Collections.Generic.List<int>();
        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                if (spawnedPlatforms[i] == null || spawnedPlatforms[j] == null) continue;
                float d = Vector3.Distance(spawnedPlatforms[i].transform.position, spawnedPlatforms[j].transform.position);
                if (d <= jumpMaxDistance)
                {
                    adj[i].Add(j);
                    adj[j].Add(i);
                }
            }
        }

        // BFS
        var q = new System.Collections.Generic.Queue<int>();
        q.Enqueue(0); visited[0] = true; int count = 1;
        while (q.Count > 0)
        {
            int u = q.Dequeue();
            foreach (var v in adj[u])
            {
                if (!visited[v]) { visited[v] = true; q.Enqueue(v); count++; }
            }
        }
        return count == n;
    }

    private void TryEnsureConnectivity()
    {
        if (spawnedPlatforms == null || spawnedPlatforms.Count == 0) return;
        int attempts = 0;
        while (!IsGraphConnected() && attempts < maxBridgeAttempts)
        {
            // find components and bridge the closest pair
            var comps = GetComponents();
            if (comps.Count <= 1) break;

            // choose two components: first and any other
            var compA = comps[0];
            var compB = comps[1];

            // find closest pair between compA and compB
            float bestD = float.MaxValue; Vector3 bestA = Vector3.zero; Vector3 bestB = Vector3.zero;
            foreach (int i in compA)
            {
                foreach (int j in compB)
                {
                    var pa = spawnedPlatforms[i]; var pb = spawnedPlatforms[j];
                    if (pa == null || pb == null) continue;
                    float d = Vector3.Distance(pa.transform.position, pb.transform.position);
                    if (d < bestD) { bestD = d; bestA = pa.transform.position; bestB = pb.transform.position; }
                }
            }

            Vector3 bridgePos = (bestA + bestB) * 0.5f;
            if (LevelManager.Instance != null && !LevelManager.Instance.IsInsideBounds(bridgePos)) break;

            // spawn intermediate platform
            Platform newP = null;
            if (platformFactory != null)
            {
                newP = platformFactory.SpawnPlatformAt(bridgePos, platformDefinitionId, platformDefinitionIndex);
            }
            else
            {
                Debug.LogWarning("GridLevelGenerator: no PlatformFactory to spawn bridge platform.");
                break;
            }

            if (newP != null) spawnedPlatforms.Add(newP);
            attempts++;
            // rebuild graph in level manager
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.BuildPlatformGraph();
                LevelManager.Instance.AssignNeighborsToExistingEnemies();
            }
        }
    }

    private System.Collections.Generic.List<int>[] GetAdjacency()
    {
        int n = spawnedPlatforms.Count;
        var adj = new System.Collections.Generic.List<int>[n];
        for (int i = 0; i < n; i++) adj[i] = new System.Collections.Generic.List<int>();
        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                if (spawnedPlatforms[i] == null || spawnedPlatforms[j] == null) continue;
                float d = Vector3.Distance(spawnedPlatforms[i].transform.position, spawnedPlatforms[j].transform.position);
                if (d <= jumpMaxDistance)
                {
                    adj[i].Add(j);
                    adj[j].Add(i);
                }
            }
        }
        return adj;
    }

    private System.Collections.Generic.List<System.Collections.Generic.List<int>> GetComponents()
    {
        var comps = new System.Collections.Generic.List<System.Collections.Generic.List<int>>();
        if (spawnedPlatforms == null || spawnedPlatforms.Count == 0) return comps;
        int n = spawnedPlatforms.Count;
        var adj = GetAdjacency();
        var vis = new bool[n];
        for (int i = 0; i < n; i++)
        {
            if (vis[i]) continue;
            var comp = new System.Collections.Generic.List<int>();
            var q = new System.Collections.Generic.Queue<int>();
            q.Enqueue(i); vis[i] = true;
            while (q.Count > 0)
            {
                int u = q.Dequeue(); comp.Add(u);
                foreach (var v in adj[u]) if (!vis[v]) { vis[v] = true; q.Enqueue(v); }
            }
            comps.Add(comp);
        }
        return comps;
    }
}


