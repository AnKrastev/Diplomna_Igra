using UnityEngine;
using UnityEngine.UI;

public class LungeBar : MonoBehaviour
{
    [Header("Lunge Bar UI")]
    [SerializeField] private Image lungeUI;
    [SerializeField] private CanvasGroup lunge_bar_group;

    [Header("Lunge Bar Parameters")]
    [SerializeField] private float idle_opacity = 0.5f;
    [SerializeField] private float active_opacity = 1f;


    public void UpdateBar(float current, float max, bool isCharging)
    {
        lungeUI.fillAmount = current / max;
        lunge_bar_group.alpha = isCharging ? active_opacity : idle_opacity;
    }
}
