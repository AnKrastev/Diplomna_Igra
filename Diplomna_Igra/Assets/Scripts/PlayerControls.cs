using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerControls : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] public float max_speed = 8f;      
    [SerializeField] public float sprint_speed = 12f;
    [SerializeField] private float acceleration = 80f;
    [SerializeField] private float deceleration = 40f;

    [Header("Jump")]
    [SerializeField] private float jump_force = 6f;
    [SerializeField] private Transform ground_check;
    [SerializeField] private LayerMask ground_layer;

    [Header("Lunge")]
    [SerializeField] private float lunge_speed_while_charging = 4f;
    [SerializeField] private float lunge_max_duration = 3f;
    [SerializeField] private float lunge_force_per_second = 10f;
    [SerializeField] private float lunge_arc_height = 0.4f;

    private Rigidbody rb;
    private Transform camera;
    private Vector2 move_input;
    private CapsuleCollider collider;
    private bool isGrounded;
    public StaminaBar _staminaController;
    private bool is_lunging = false;
    private float lunge_timer = 0f;



    void Start()
    {
        rb = GetComponent<Rigidbody>();
        collider = GetComponent<CapsuleCollider>();
        camera = Camera.main.transform;
        _staminaController = GetComponent<StaminaBar>();

        rb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        PlayerRotation();
        CheckGround();
        Movement();
        if (_staminaController.sprinting && !is_lunging)
            _staminaController.Sprinting();

        if (is_lunging) 
        {
            lunge_timer = Mathf.Min(lunge_timer + Time.fixedDeltaTime, lunge_max_duration);
        }
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
            ground_layer,
            QueryTriggerInteraction.Ignore
        );

    }

    void Movement()
    {
        float currentMax;

        if (is_lunging)
        {
            currentMax = lunge_speed_while_charging;
        }
        else if (_staminaController.sprinting)
        {
            currentMax = sprint_speed;
        }
        else
        {
            currentMax = max_speed;
        }

        Vector3 move = transform.right * move_input.x + transform.forward * move_input.y;

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
        if (is_lunging) 
        {
            return;
        }
        else
        {
            if (value.isPressed && isGrounded && _staminaController.StaminaJump())
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                rb.AddForce(Vector3.up * jump_force, ForceMode.VelocityChange);
            }
        }
        
    }

    void OnRun(InputValue value)
    {
        if (value.isPressed)
            _staminaController.sprinting = true;
        else
            _staminaController.sprinting = false;
    }

    void OnMove(InputValue value)
    {
        move_input = value.Get<Vector2>();
    }

    void OnLunge(InputValue value)
    {
        if (value.isPressed)
        {
            is_lunging = true;
            lunge_timer = 0f;
        }
        else 
        {
            if(is_lunging)
            {
                Vector3 lungeDirection = (transform.forward + Vector3.up * lunge_arc_height).normalized;
                rb.AddForce(lungeDirection * lunge_force_per_second * lunge_timer, ForceMode.VelocityChange);
                is_lunging = false;
                _staminaController.sprinting = false;
            }
        }
    }
}
