using UnityEngine;
using UnityEngine.SceneManagement;
public class GameOverManager : MonoBehaviour
{
    public void RestartGame()
    {
        Debug.Log("Clicked");
        // Load the game scene
        SceneManager.LoadScene("SpitFireSimulator");

        // Optionally, unload the current Game Over scene if needed
        SceneManager.UnloadSceneAsync("GameOver");
    }
}