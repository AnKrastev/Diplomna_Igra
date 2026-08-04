using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerControls : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] public float maxSpeed = 8f;      
    [SerializeField] public float sprintSpeed = 12f;
    [SerializeField] private float acceleration = 80f;
    [SerializeField] private float deceleration = 40f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody rb;
    private Transform camera;
    private Vector2 moveInput;
    private CapsuleCollider collider;
    private bool isGrounded;
    public StaminaBar _staminaController;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<CapsuleCollider>();
        camera = Camera.main.transform;
        _staminaController = GetComponent<StaminaBar>();

        rb.freezeRotation = true;

        //cursor in middle and hidden
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void FixedUpdate()
    {
        PlayerRotation();
        CheckGround();
        Movement();
        if (_staminaController.sprinting)
            _staminaController.Sprinting();
    }


    void PlayerRotation() 
    {
        //rotate the player with the camera
        transform.rotation = Quaternion.Euler(0f, camera.eulerAngles.y, 0f);
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
        float currentMax = _staminaController.sprinting ? sprintSpeed : maxSpeed;

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

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

        if (horizontalVel.magnitude > currentMax)
        {
            Vector3 clamped = horizontalVel.normalized * currentMax;
            rb.linearVelocity = new Vector3(clamped.x, rb.linearVelocity.y, clamped.z);
        }
    }


    void OnJump(InputValue value)
    {
        if (value.isPressed && isGrounded && _staminaController.StaminaJump())
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        }
    }

    void OnRun(InputValue value)
    {
        Debug.Log($"OnSprint fired, isPressed={value.isPressed}");
        if (value.isPressed)
            _staminaController.sprinting = true;
        else
            _staminaController.sprinting = false;
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
}
