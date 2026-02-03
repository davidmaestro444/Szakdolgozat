using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void PlayVsAI()
    {
        SceneManager.LoadScene("PVEScene");
    }

    public void PlayVsPlayer()
    {
        SceneManager.LoadScene("PVPScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
