using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlatformConfig", menuName = "Platform/PlatformConfiguration", order = 1)]
public class PlateformsData : ScriptableObject
{
    [System.Serializable]
    public class PlatformDefinition
    {
        [Tooltip("Unique identifier for this platform definition")]
        public string id;

        [Tooltip("Boolean to know if this platform moves")]
        public bool moves = false;

        [Tooltip("Movement speed (units/s)")]
        public float speed = 5f;

        [Tooltip("Boolean to know if this platform will disappear when stepped on")]
        public bool isTemporary = false;

        [Tooltip("Represents the direction of the movement of the platform")]
        public Vector3 dir = Vector3.right;

        [Tooltip("Amplitude of movement from the start position")]
        public float movementRange = 3f;

        [Tooltip("Delay before disappearing after player steps on it")]
        public float disappearDelay = 0.5f;

        [Tooltip("Time before respawn (<=0 means no respawn)")]
        public float respawnTime = 3f;
    }

    [SerializeField]
    private List<PlatformDefinition> definitions = new List<PlatformDefinition>();

    public IReadOnlyList<PlatformDefinition> Definitions => definitions;

    public PlatformDefinition GetById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        return definitions.Find(d => d.id == id);
    }

    public PlatformDefinition GetAt(int index)
    {
        if (index < 0 || index >= definitions.Count) return null;
        return definitions[index];
    }

    public PlatformDefinition GetRandom()
    {
        if (definitions == null || definitions.Count == 0) return null;
        return definitions[Random.Range(0, definitions.Count)];
    }

    public bool HasValidEntries()
    {
        return definitions != null && definitions.Count > 0;
    }
}
