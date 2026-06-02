using UnityEngine;
using UnityEngine.SceneManagement;

public class menu : MonoBehaviour
{
    public void PlayGame()
    {
        // Continue or play from current progress
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void NewPlay()
    {
        // Start a fresh game
        // You can add logic here to reset scores or progress if needed
        Debug.Log("Starting New Game...");
        SceneManager.LoadScene(1); // Usually Scene 1 is the first level
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}
