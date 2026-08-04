using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
    [Header("Stamina Parameters")]
    public float playerStamina = 100f;
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float jumpCost = 15f;
    [HideInInspector] public bool regenarated = true;
    [HideInInspector] public bool sprinting = false;

    [Header("Stamina Update Paramteres")]
    [Range(0f, 100f)][SerializeField] private float drain = 0.5f;
    [Range(0f, 100f)][SerializeField] private float regen = 0.5f;

    [Header("Stamina Bar UI")]
    [SerializeField] private Image staminaUI = null;
    [SerializeField] private CanvasGroup sliderGroup = null;

    private PlayerControls playerControls;
    private float maxRunSpeed;
    private float walkSpeed;

    private void Start()
    {
        playerControls = GetComponent<PlayerControls>();
        playerStamina = maxStamina;
        maxRunSpeed = playerControls.maxSpeed;
        walkSpeed = playerControls.maxSpeed * 0.5f;
    }

    private void Update()
    {
        if (!sprinting) 
        {
            if (playerStamina <= maxStamina - 0.01f) 
            {
                playerStamina += regen * Time.deltaTime;
                UpdateStaminaUI(1);

                if(playerStamina >= maxStamina)
                {
                    playerStamina = maxStamina;
                    regenarated = true;
                }
            }

        }
    }

    void UpdateStaminaUI(int value)
    {
        staminaUI.fillAmount = playerStamina / maxStamina;

        if (value == 0)
        {
            sliderGroup.alpha = 0f;
        }
        else
        {
            sliderGroup.alpha = 1f;
        }
    }

    public bool StaminaJump()
    {
        if (playerStamina >= jumpCost)
        {
            playerStamina -= jumpCost;
            UpdateStaminaUI(1);
            return true;
        }
        return false;
    }

    public void Sprinting() 
    {
        if (regenarated && playerStamina > 0f)
        { 
            playerStamina -= drain * Time.deltaTime;
            UpdateStaminaUI(1);

            if (playerStamina <= 0f)
            {
                playerStamina = 0f;
                regenarated = false;
                sprinting = false; 
            }
        }
        else
        {
            sprinting = false;
        }
    }
}

