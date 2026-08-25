using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int enemies_till_win = 4;
    [SerializeField] private int targets_till_lose = 20;

    [SerializeField] private int enemies_hit;
    [SerializeField] private int targets_collected;

    [Header("UI Panels")]
    [SerializeField] private GameObject gameplay;
    [SerializeField] private GameObject win_panel;
    [SerializeField] private GameObject lose_panel;

    private bool game_over = false;
    public TextMeshProUGUI enemies_hit_text;
    public TextMeshProUGUI targets_collected_text;


    void FixedUpdate()
    {
        CheckGameOver();
    }



    public void EnemyIncrement()
    {
        enemies_hit++; 
        enemies_hit_text.text = $"Enemies Hit: {enemies_hit}/{enemies_till_win}";
    }

    public void TargetIncrement()
    {
        targets_collected++;
        targets_collected_text.text = $"Targets Collected: {targets_collected}/{targets_till_lose}";

    }

    private void CheckGameOver() 
    {
        if (game_over) 
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f;
            game_over = false;
        }
        else if(enemies_hit >= enemies_till_win)
        {
            gameplay.SetActive(false);
            win_panel.SetActive(true);
            game_over = true;
        }
        else if (targets_collected >= targets_till_lose)
        {
            gameplay.SetActive(false);
            lose_panel.SetActive(true);
            game_over = true;
        }
    }

    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        enemies_hit_text.text = $"Enemies Hit: {enemies_hit}/{enemies_till_win}";
        targets_collected_text.text = $"Targets Collected: {targets_collected}/{targets_till_lose}";
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadSceneAsync("Gameplay");
    }

    public void BackToMenu()
    {
        Time.timeScale = 0f;
        SceneManager.LoadSceneAsync("MainMenu");
    }
}
