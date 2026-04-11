using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public Image backgroundImage;
    public Sprite[] animationFrames;
    public float frameRate = 0.5f;
    public Sprite emptyBackground;
    public TextMeshProUGUI pressAnyKeyText;
    public float blinkSpeed = 1.5f;
    public GameObject pvpButton;
    public GameObject pveButton;
    public GameObject quitButton;
    private bool isWaitingForInput = false;

    private void Start()
    {
        pvpButton.SetActive(false);
        pveButton.SetActive(false);
        quitButton.SetActive(false);
        pressAnyKeyText.gameObject.SetActive(false);

        if (animationFrames.Length > 0 && backgroundImage != null)
        {
            StartCoroutine(PlayIntroAnimation());
        }
    }

    private IEnumerator PlayIntroAnimation()
    {
        for (int i = 0; i < animationFrames.Length; i++)
        {
            backgroundImage.sprite = animationFrames[i];
            yield return new WaitForSeconds(frameRate);
        }

        pressAnyKeyText.gameObject.SetActive(true);
        isWaitingForInput = true;
    }

    private void Update()
    {
        if (isWaitingForInput)
        {
            Color textColor = pressAnyKeyText.color;
            textColor.a = Mathf.PingPong(Time.time * blinkSpeed, 1f);
            pressAnyKeyText.color = textColor;

            if (Input.anyKeyDown)
            {
                GoToGameModeSelector();
            }
        }
    }

    private void GoToGameModeSelector()
    {
        isWaitingForInput = false;
        pressAnyKeyText.gameObject.SetActive(false);

        backgroundImage.sprite = emptyBackground;

        pvpButton.SetActive(true);
        pveButton.SetActive(true);
        quitButton.SetActive(true);
    }

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
