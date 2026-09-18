using System;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    [SerializeField] private float health = 100f;
    [SerializeField] private float speed = 10f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private int damage = 5;

    [SerializeField] private bool isGrounded = true;

    private Rigidbody rb;
    private Vector3 moveInput = Vector3.zero;
    // La gestion de la caméra a été déplacée vers CameraController

    public float Speed
    {
        get { return speed; }
        set { speed = value; }
    }

    public float Health
    {
        get { return health; }
        set { health = value; }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogWarning("PlayerBehaviour: Rigidbody not found. Movement and jump require a Rigidbody.");
        }
        // La gestion de la caméra est gérée par CameraController attaché à la caméra
    }

    void Update()
    {
        ReadInput();

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            HandleJump();
        }

        if(Input.GetMouseButtonDown(0)) { }
    }

    void FixedUpdate()
    {
        ApplyMovement();
    }

    // Lit les entrées et stocke le vecteur de mouvement
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

    // Applique le mouvement en FixedUpdate pour la physique
    private void ApplyMovement()
    {
        if (rb != null)
        {
            Vector3 move = moveInput * speed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + move);
        }
        else
        {
            // Fallback kinematic movement si Rigidbody absent
            transform.position += moveInput * speed * Time.deltaTime;
        }
    }

    private void HandleJump()
    {
        if (rb != null)
        {
            // Impulsion verticale
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
        else
        {
            // Fallback simple
            transform.position += Vector3.up * jumpForce * Time.deltaTime;
        }
        isGrounded = false;
    }

    // Détecte si le joueur touche le sol via les collisions
    private void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            // Si le contact a une normale principalement vers le haut, on considère le sol
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                return;
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                return;
            }
        }
        isGrounded = false;
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }

    // Tourne la caméra en fonction de la souris.
    // Yaw: rotation du joueur autour de l'axe Y.
    // Pitch: inclinaison de la caméra (clampée).
    // La gestion de la caméra est externalisée dans CameraController.cs
}
