using System.Collections;
using UnityEngine;


[RequireComponent(typeof(Collider))]
public class Platform : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private PlateformsData config;
    [Tooltip("Choisir une définition par index dans l'asset (prioritaire si >=0)")]
    [SerializeField] private int definitionIndex = -1;
    [Tooltip("Ou choisir une définition par id (prioritaire si non vide)")]
    [SerializeField] private string definitionId = "";

    // Fallback local values si aucun config/definition n'est assigné
    [SerializeField] private bool moves = false;
    [SerializeField] private float speed = 1f;
    [SerializeField] private Vector3 dir = Vector3.right;
    [SerializeField] private bool isTemporary = false;

    [Header("Movement")]
    [Tooltip("Amplitude du mouvement (distance maximale depuis la position de départ)")]
    [SerializeField] private float movementRange = 3f;

    [Header("Temporary platform")]
    [Tooltip("Délai avant la disparition une fois le joueur sur la plateforme")]
    [SerializeField] private float disappearDelay = 0.5f;
    [Tooltip("Temps avant réapparition (si <= 0, la plateforme reste désactivée)")]
    [SerializeField] private float respawnTime = 3f;

    private Vector3 startPosition;
    private float elapsed = 0f;
    private bool triggered = false;
    private Collider platformCollider;
    private Renderer[] renderers;

    void Awake()
    {
        platformCollider = GetComponent<Collider>();
        renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
    }

    private PlateformsData.PlatformDefinition selectedDefinition;

    void Start()
    {
        startPosition = transform.position;

        // Sélectionne la definition depuis l'asset si disponible
        if (config != null)
        {
            if (!string.IsNullOrEmpty(definitionId))
            {
                selectedDefinition = config.GetById(definitionId);
            }
            else if (definitionIndex >= 0)
            {
                selectedDefinition = config.GetAt(definitionIndex);
            }
            else if (config.HasValidEntries())
            {
                // par défaut, prends la première
                selectedDefinition = config.GetAt(0);
            }
        }

        // Si une definition est sélectionnée, écrase les valeurs locales
        if (selectedDefinition != null)
        {
            moves = selectedDefinition.moves;
            speed = selectedDefinition.speed;
            dir = selectedDefinition.dir;
            isTemporary = selectedDefinition.isTemporary;
            movementRange = selectedDefinition.movementRange;
            disappearDelay = selectedDefinition.disappearDelay;
            respawnTime = selectedDefinition.respawnTime;
        }
    }

    void Update()
    {
        var defMoves = moves;
        var defSpeed = speed;
        var defDir = dir;
        var defTemporary = isTemporary;

        if (config != null)
        {

            var t = config.GetType();
            var field = t.GetField("PlaterformDefinition");

            try
            {
                var movesField = t.GetField("moves");
                var speedField = t.GetField("speed");
                var dirField = t.GetField("dir");
                var tempField = t.GetField("isTemporary");
                if (movesField != null) defMoves = (bool)movesField.GetValue(config);
                if (speedField != null) defSpeed = (float)speedField.GetValue(config);
                if (dirField != null) defDir = (Vector3)dirField.GetValue(config);
                if (tempField != null) defTemporary = (bool)tempField.GetValue(config);
            }
            catch
            {
                // Si reflection échoue, on tombe sur les valeurs locales
            }
        }

        if (defMoves)
        {
            elapsed += Time.deltaTime;
            Vector3 norm = defDir.sqrMagnitude > 0.0001f ? defDir.normalized : Vector3.right;
            // Oscillation sinusoidale autour de startPosition
            Vector3 offset = norm * Mathf.Sin(elapsed * defSpeed) * movementRange;
            transform.position = startPosition + offset;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isTemporary && config == null)
            return;

        bool temp = isTemporary;
        if (config != null)
        {
            var t = config.GetType();
            var tempField = t.GetField("isTemporary");
            try
            {
                if (tempField != null) temp = (bool)tempField.GetValue(config);
            }
            catch { }
        }

        if (!temp) return;

        if (collision.gameObject.CompareTag("Player") && !triggered)
        {
            StartCoroutine(TemporarySequence());
        }
    }

    private IEnumerator TemporarySequence()
    {
        triggered = true;
        // attente avant disparition
        yield return new WaitForSeconds(disappearDelay);

        // Désactive l'interaction et le rendu
        if (platformCollider != null) platformCollider.enabled = false;
        foreach (var r in renderers)
        {
            if (r != null) r.enabled = false;
        }

        if (respawnTime > 0f)
        {
            yield return new WaitForSeconds(respawnTime);

            // Réactiver
            if (platformCollider != null) platformCollider.enabled = true;
            foreach (var r in renderers)
            {
                if (r != null) r.enabled = true;
            }
            triggered = false;
        }
        else
        {
            // Si respawnTime <= 0, on laisse la plateforme désactivée définitivement
            
            gameObject.SetActive(false);
        }
    }
}
