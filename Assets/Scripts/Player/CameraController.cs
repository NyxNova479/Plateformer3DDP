using UnityEngine;


public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.6f, 0f);

    [Header("Mouse Look")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxPitch = 85f;
    [SerializeField] private bool lockCursor = true;

    [Header("Distance / Collision")]
    [SerializeField] private float distance = 3f;
    [SerializeField] private float minDistance = 0.5f;
    [SerializeField] private float collisionBuffer = 0.1f;
    [SerializeField] private LayerMask collisionMask = ~0; // tout par défaut
    [SerializeField] private float smoothSpeed = 10f;

    private float yaw = 0f;
    private float pitch = 0f;
    private float currentDistance;

    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("CameraController: target not assigned.");
        }

        currentDistance = distance;

        // Initialisation des angles selon la caméra et la cible
        Vector3 e = transform.eulerAngles;
        pitch = e.x;
        yaw = target != null ? target.eulerAngles.y : e.y;

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        HandleMouseInput();
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            lockCursor = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Calcul de la rotation désirée
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        // Calcul de la position désirée de la caméra (derrière la cible)
        Vector3 desiredCamPos = target.position + targetOffset - rotation * Vector3.forward * distance;

        // Collision : tirer un rayon depuis la target vers la position désirée
        Vector3 dir = (desiredCamPos - (target.position + targetOffset)).normalized;
        float maxDist = Vector3.Distance(target.position + targetOffset, desiredCamPos);

        if (Physics.SphereCast(target.position + targetOffset, 0.2f, dir, out RaycastHit hit, maxDist, collisionMask, QueryTriggerInteraction.Ignore))
        {
            float hitDist = Mathf.Max(hit.distance - collisionBuffer, minDistance);
            currentDistance = Mathf.Lerp(currentDistance, hitDist, Time.deltaTime * smoothSpeed);
        }
        else
        {
            currentDistance = Mathf.Lerp(currentDistance, distance, Time.deltaTime * smoothSpeed);
        }

        Vector3 finalPos = target.position + targetOffset - rotation * Vector3.forward * currentDistance;

        // Appliquer position et rotation
        transform.position = Vector3.Lerp(transform.position, finalPos, Time.deltaTime * smoothSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * smoothSpeed);

        // Appliquer la rotation horizontale au target (yaw) pour que le joueur suive la vue
        Vector3 targetEuler = target.eulerAngles;
        targetEuler.y = yaw;
        target.eulerAngles = targetEuler;
    }

    private void HandleMouseInput()
    {
        float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw += mx;
        pitch -= my;
        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);
    }

    // Permet d'assigner la cible dynamiquement
    public void SetTarget(Transform t)
    {
        target = t;
    }
}
