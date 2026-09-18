using UnityEngine;

[CreateAssetMenu(fileName = "PlateformConfig", menuName = "Plateform/PlateformConfiguration", order = 1)]
public class PlateformsData : ScriptableObject
{
    [System.Serializable]
    public class PlaterformDefinition
    {
        [Tooltip("Boolean to know if this plateform moves")]
        public bool moves = false;

        [Tooltip("Movement speed (units/s)")]
        public float speed = 5f;

        [Tooltip("Boolean to know if this plateform will disapear")]
        public bool isTemporary = false;

        [Tooltip("Represents the direction of the movement of the plateform")]
        public Vector3 dir = Vector3.zero;

    }
}
