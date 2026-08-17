using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    [SerializeField] private int enemies_till_win = 4;
    [SerializeField] private int targets_till_lose = 20;

    [SerializeField] private int enemies_hit;
    [SerializeField] private int targets_collected;

    [SerializeField] private GameObject gameplay;
    [SerializeField] private GameObject win_panel;
    [SerializeField] private GameObject lose_panel;

    private bool game_over = false;

    public void EnemyIncrement()
    {
        enemies_hit++;
        CheckGameOver();
    }

    public void TargetIncrement()
    {
        targets_collected++;
        CheckGameOver();
    }

    private void CheckGameOver() 
    {
        if (game_over) 
        {
            return;
        }
        else if(enemies_hit >= enemies_till_win)
        {
            Debug.Log("Game Over: You Win!");
            gameplay.SetActive(false);
            win_panel.SetActive(true);
            game_over = true;
        }
        else if (targets_collected >= targets_till_lose)
        {
            Debug.Log("Game Over: You Lose!");
            gameplay.SetActive(false);
            lose_panel.SetActive(true);
            game_over = true;
        }

        if(game_over)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f;
        }
    }

    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
