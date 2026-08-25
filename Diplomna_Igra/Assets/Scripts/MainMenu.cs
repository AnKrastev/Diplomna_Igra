using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public void StartGameButton()
    {
        SceneManager.LoadSceneAsync("Gameplay");
        Time.timeScale = 1f;
    }


}
