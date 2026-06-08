using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverScreen : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip gameOverMusic;      // Music to play when game over screen appears
    public AudioClip clickSound;         // Sound to play when clicking to continue
    private AudioSource audioSource;

    [Header("Transition Settings")]
    public string mainMenuScene = "MainMenu";  // Your main menu scene name
    public float inputDelay = 2f;              // How long to wait before accepting input
    public float fadeDuration = 1f;            // Fade to black duration

    [Header("Fade Panel")]
    public Image fadePanel;                    // Optional fade panel in the scene

    private bool canInput = false;
    private bool isTransitioning = false;

    void Start()
    {
        Debug.Log("GameOverScreen started");

        // Set up audio
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }

        // Play game over music
        if (gameOverMusic != null)
        {
            audioSource.clip = gameOverMusic;
            audioSource.volume = 1f;
            audioSource.Play();
        }

        // Start the input delay timer
        StartCoroutine(EnableInputAfterDelay());
    }

    IEnumerator EnableInputAfterDelay()
    {
        Debug.Log("Starting input delay: " + inputDelay + " seconds");
        yield return new WaitForSeconds(inputDelay);
        canInput = true;
        Debug.Log("Input is now enabled");
    }

    void Update()
    {
        if (canInput && !isTransitioning)
        {
            // Check for various input methods
            if (Input.GetMouseButtonDown(0) ||
                Input.GetKeyDown(KeyCode.Space) ||
                Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("Input detected, continuing to main menu...");
                ContinueToMainMenu();
            }
        }
    }

    void ContinueToMainMenu()
    {
        if (isTransitioning)
        {
            Debug.Log("Already transitioning, ignoring input");
            return;
        }

        isTransitioning = true;

        // Play click sound if available
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
            StartCoroutine(DelayedTransition());
        }
        else
        {
            StartCoroutine(DelayedTransition());
        }
    }

    IEnumerator DelayedTransition()
    {
        // If we have a fade panel in the scene, use it
        if (fadePanel != null)
        {
            // Fade to black
            Color startColor = fadePanel.color;
            Color endColor = Color.black;

            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                float progress = t / fadeDuration;
                fadePanel.color = Color.Lerp(startColor, endColor, progress);
                yield return null;
            }
            fadePanel.color = endColor;
        }
        else
        {
            // No fade panel, just wait briefly
            yield return new WaitForSeconds(0.5f);
        }

        // Load main menu scene directly
        Debug.Log("Loading main menu scene: " + mainMenuScene);
        SceneManager.LoadScene(mainMenuScene);
    }
}