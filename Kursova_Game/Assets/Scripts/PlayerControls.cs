using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerControls : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float acceleration = 80f;
    [SerializeField] private float deceleration = 40f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;


    private Transform cameraTransform;
    private Rigidbody rb;
    private Vector2 moveInput;
    private CapsuleCollider collider;
    private const float GroundCheckDistance = 0.15f;
    private const float GroundCheckRadius = 0.25f;
    private bool isGrounded;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<CapsuleCollider>();
        rb.freezeRotation = true;
        cameraTransform = Camera.main.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void FixedUpdate()
    {
        PlayerRotation();
        CheckGround();
        Movement();
    }


    void PlayerRotation() 
    {
        float cameraYaw = cameraTransform.eulerAngles.y;
        transform.rotation = Quaternion.Euler(0f, cameraYaw, 0f);
    }

    void CheckGround()
    {
        Vector3 feetPos = transform.position + collider.center + Vector3.down * (collider.height / 2f - collider.radius);

        isGrounded = Physics.CheckSphere(
            feetPos,
            collider.radius + 0.05f,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );

        Debug.DrawRay(feetPos, Vector3.down * 0.1f, isGrounded ? Color.green : Color.red);
    }

    void Movement()
    {
        float cameraYaw = cameraTransform.eulerAngles.y;
        Quaternion yawRotation = Quaternion.Euler(0f, cameraYaw, 0f);
        Vector3 move = yawRotation * new Vector3(moveInput.x, 0f, moveInput.y);

        if (move.magnitude > 0.1f)
        {
            rb.AddForce(move.normalized * acceleration * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(-flatVel * deceleration * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }

        Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (horizontalVel.magnitude > maxSpeed)
        {
            Vector3 clamped = horizontalVel.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(clamped.x, rb.linearVelocity.y, clamped.z);
        }
    }

    void OnJump(InputValue value)
    {
        if (value.isPressed && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        }
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
}
