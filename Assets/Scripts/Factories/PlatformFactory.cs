using UnityEngine;

public class PlatformFactory : MonoBehaviour
{
    [Header("Platform Prefab")]
    [SerializeField] private GameObject[] platformPrefabs; // doit contenir le composant Platform
    [SerializeField] private PlateformsData platformData;

    /// <summary>
    /// Crée une plateforme à la position donnée et applique la définition choisie (par id ou index)
    /// </summary>
    public Platform SpawnPlatformAt(Vector3 position, string definitionId = "", int definitionIndex = -1)
    {
        if (platformPrefabs == null)
        {
            Debug.LogWarning("PlatformFactory: platformPrefab not set.");
            return null;
        }

        if (LevelManager.Instance != null && !LevelManager.Instance.IsInsideBounds(position))
        {
            Debug.LogWarning("PlatformFactory: spawn position outside bounds, aborting spawn.");
            return null;
        }

        int rand = Random.Range(0, platformPrefabs.Length);

        GameObject go = Instantiate(platformPrefabs[rand], position, Quaternion.identity);
        Platform p = go.GetComponent<Platform>();
        if (p != null)
        {
            if (platformData != null)
            {
                // apply selected definition if provided
                PlateformsData.PlatformDefinition def = null;
                if (!string.IsNullOrEmpty(definitionId))
                    def = platformData.GetById(definitionId);
                else if (definitionIndex >= 0)
                    def = platformData.GetAt(definitionIndex);

                if (def != null)
                {
                    p.ApplyDefinition(def);
                }
            }

            // Rebuild platform graph
            if (LevelManager.Instance != null)
                LevelManager.Instance.BuildPlatformGraph();
        }

        return p;
    }
}
