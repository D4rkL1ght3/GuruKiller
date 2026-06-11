using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("References")]
    public PlayerController playerController;
    public TeacherAI teacherAI;

    [Header("Jumpscare UI")]
    public GameObject jumpscarePanel;
    public AudioSource audioSource;
    public AudioClip jumpscareSound;
    public float jumpscareDuration = 1.25f;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;

    [Header("Restart")]
    public bool allowRestartWithR = true;
    public KeyCode restartKey = KeyCode.R;

    private bool gameOverActive;

    void Start()
    {
        if (jumpscarePanel != null)
        {
            jumpscarePanel.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (!gameOverActive)
            return;

        if (allowRestartWithR && Input.GetKeyDown(restartKey))
        {
            RestartCurrentScene();
        }
    }

    public void TriggerGameOver()
    {
        if (gameOverActive)
            return;

        StartCoroutine(GameOverRoutine());
    }

    private IEnumerator GameOverRoutine()
    {
        gameOverActive = true;

        Time.timeScale = 1f;

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        if (teacherAI != null)
        {
            teacherAI.enabled = false;
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (jumpscarePanel != null)
        {
            jumpscarePanel.SetActive(true);
        }

        if (audioSource != null && jumpscareSound != null)
        {
            audioSource.PlayOneShot(jumpscareSound);
        }

        yield return new WaitForSecondsRealtime(jumpscareDuration);

        if (jumpscarePanel != null)
        {
            jumpscarePanel.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    public void RestartCurrentScene()
    {
        Time.timeScale = 1f;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    public void ReturnToMainMenu(string mainMenuSceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}