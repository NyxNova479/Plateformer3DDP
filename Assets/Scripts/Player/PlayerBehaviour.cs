using System;
using System.Collections;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    [Header("Stats de base")]
    [SerializeField] private float health = 100f;
    [SerializeField] private float speed = 10f;
    [SerializeField] private float jumpForce = 5f;

    [Header("Combat & Spin Attack")]
    [SerializeField] private float attackDamage = 25f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float spinDuration = 0.6f;       // Durée de l'attaque tournoyante
    [SerializeField] private float spinRotationSpeed = 1200f; // Vitesse de rotation (degrés/sec)
    [SerializeField] private float spinKillRadius = 1.5f;     // Rayon d'impact de l'attaque tournoyante
    [SerializeField] private Transform rendererChild;         // L'enfant visuel du joueur (mesh/modèle 3D)

    private float lastAttackTime = -999f;
    private bool isSpinning = false;

    [SerializeField] private bool isGrounded = true;

    public float coyoteTime = 0.5f;
    private float coyoteTimer;

    private Rigidbody rb;
    private Vector3 moveInput = Vector3.zero;
    private MovingPlatformRider platformRider;

    public float Speed
    {
        get { return speed; }
        set { speed = value; }
    }

    public float Health
    {
        get { return health; }
        set
        {
            if (Mathf.Approximately(health, value)) return;
            health = value;
            OnHealthChanged?.Invoke(health);
            if (health <= 0f)
            {
                OnDeath?.Invoke();
            }
        }
    }

    public event Action<float> OnHealthChanged;
    public event Action OnDeath;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogWarning("PlayerBehaviour: Rigidbody not found. Movement and jump require a Rigidbody.");
        }
        platformRider = GetComponent<MovingPlatformRider>();
        if (platformRider == null) platformRider = gameObject.AddComponent<MovingPlatformRider>();
    }

    void Update()
    {
        ReadInput();

        CheckIfGrounded();

        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Space) && coyoteTimer > 0f)
        {
            HandleJump();
            coyoteTimer = 0;
        }

        // Clic gauche (ou touche d'attaque) déclenche le Spin
        if (Input.GetMouseButtonDown(0))
        {
            TryAttack();
        }
    }

    private void CheckIfGrounded()
    {
        float rayLength = 1.2f;
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, rayLength))
        {
            Collider col = hit.collider;
            if (col != null && col.enabled && (col.CompareTag("Platform") || col.GetComponentInParent<Platform>() != null))
            {
                isGrounded = true;
                return;
            }
        }
        isGrounded = false;
    }

    void FixedUpdate()
    {
        ApplyMovement();
    }

    private void TryAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown || isSpinning) return;
        lastAttackTime = Time.time;

        // Lance la coroutine du tournoiement
        StartCoroutine(SpinAttackRoutine());
    }

    private IEnumerator SpinAttackRoutine()
    {
        isSpinning = true;
        float elapsed = 0f;

        Vector3 originalScale = rendererChild != null ? rendererChild.localScale : Vector3.one;
        Vector3 spinScale = new Vector3(originalScale.x * 1.2f, originalScale.y * 0.8f, originalScale.z * 1.2f);

        while (elapsed < spinDuration)
        {
            elapsed += Time.deltaTime;

            // Rotation rapide du visuel du joueur
            if (rendererChild != null)
            {
                rendererChild.Rotate(Vector3.up, spinRotationSpeed * Time.deltaTime, Space.World);
                rendererChild.localScale = Vector3.Lerp(rendererChild.localScale, spinScale, 15f * Time.deltaTime);
            }

            // Dégâts sur les ennemis dans le rayon du tournoiement
            Collider[] hits = Physics.OverlapSphere(transform.position, spinKillRadius);
            foreach (var hit in hits)
            {
                if (hit == null) continue;
                Enemy enemy = hit.GetComponentInParent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(attackDamage);
                }
            }

            yield return null;
        }

        // Remet le visuel en place à la fin de l'attaque
        if (rendererChild != null)
        {
            rendererChild.localRotation = Quaternion.identity;
            rendererChild.localScale = originalScale;
        }

        isSpinning = false;
    }

    public void TakeDamage(float amount)
    {
        Health = Mathf.Max(0f, Health - amount);
        if (Health <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        enabled = false;
        Debug.Log("Player died");
    }

    private void ReadInput()
    {
        float forward = 0f;
        float right = 0f;

        if (Input.GetKey(KeyCode.W)) forward += 1f;
        if (Input.GetKey(KeyCode.S)) forward -= 1f;
        if (Input.GetKey(KeyCode.D)) right += 1f;
        if (Input.GetKey(KeyCode.A)) right -= 1f;

        moveInput = (transform.forward * forward + transform.right * right).normalized;
    }

    private void ApplyMovement()
    {
        if (rb != null)
        {
            // Si le joueur est en train de tournoyer, on peut optionnellement ralentir ou conserver son élan
            Vector3 horizontalVel = moveInput * speed;
            rb.linearVelocity = new Vector3(horizontalVel.x, rb.linearVelocity.y, horizontalVel.z);
        }
        else
        {
            Vector3 targetPos = transform.position + moveInput * speed * Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, targetPos, 0.85f);
        }
    }

    private void HandleJump()
    {
        if (rb != null)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
        else
        {
            transform.position += Vector3.up * jumpForce * Time.deltaTime;
        }

        if (platformRider != null)
        {
            platformRider.SetPlatform(null);
        }

        isGrounded = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Platform plat = collision.gameObject.GetComponentInParent<Platform>();
        if (plat != null)
        {
            platformRider.SetPlatform(plat.transform);
        }

        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                return;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.GetComponentInParent<Platform>() != null)
        {
            platformRider.SetPlatform(null);
        }
        isGrounded = false;
    }

    private void OnDrawGizmosSelected()
    {
        // Visualise la zone d'effet du Spin dans l'éditeur
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spinKillRadius);
    }
}