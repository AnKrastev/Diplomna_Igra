using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
    [Header("Stamina Parameters")]
    [SerializeField] private float player_stamina = 100f;
    [SerializeField] private float max_stamina = 100f;
    [SerializeField] private float jump_cost = 15f;
    [HideInInspector] public bool regenerated = true;
    [HideInInspector] public bool sprinting = false;

    [Header("Stamina Update Parameters")]
    [Range(0f, 100f)][SerializeField] private float drain = 0.5f;
    [Range(0f, 100f)][SerializeField] private float regen = 0.5f;

    [Header("Stamina Bar UI")]
    [SerializeField] private Image staminaUI = null;
    [SerializeField] private CanvasGroup sliderGroup = null;

    private PlayerControls player_controls;
    private float max_runSpeed;
    private float walk_speed;

    private void Start()
    {
        player_controls = GetComponent<PlayerControls>();
        player_stamina = max_stamina;
        max_runSpeed = player_controls.max_speed;
        walk_speed = player_controls.max_speed * 0.5f;
    }

    private void Update()
    {
        if (!sprinting) 
        {
            if (player_stamina <= max_stamina - 0.01f) 
            {
                player_stamina += regen * Time.deltaTime;
                UpdateStaminaUI(1);

                if(player_stamina >= max_stamina)
                {
                    player_stamina = max_stamina;
                    regenerated = true;
                }
            }

        }
    }

    void UpdateStaminaUI(int value)
    {
        staminaUI.fillAmount = player_stamina / max_stamina ;

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
        if (player_stamina >= jump_cost)
        {
            player_stamina -= jump_cost;
            UpdateStaminaUI(1);
            return true;
        }
        return false;
    }

    public void Sprinting() 
    {
        if (regenerated && player_stamina > 0f)
        { 
            player_stamina -= drain * Time.deltaTime;
            UpdateStaminaUI(1);

            if (player_stamina <= 0f)
            {
                player_stamina = 0f;
                regenerated = false;
                sprinting = false; 
            }
        }
        else
        {
            sprinting = false;
        }
    }
}

