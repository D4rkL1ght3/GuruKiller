using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    public Image fadeOverlay;
    public TextMeshProUGUI titleText;
    public List<TextMeshProUGUI> menuButtons = new List<TextMeshProUGUI>();

    [Header("Audio")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public AudioClip backgroundMusic;
    public AudioClip buttonHoverSound;
    public AudioClip buttonClickSound;

    [Header("Fade Timings")]
    public float initialFadeInTime = 2.5f;
    public float titleAppearDelay = 1f;
    public float titleFadeTime = 3f;
    public float buttonsAppearDelay = 1f; 
    public float buttonFadeTime = 2f;
    public float timeBetweenButtons = 0.3f;
    public float sceneTransitionFadeTime = 1.5f;

    [Header("Button Colors")]
    public Color normalColor = Color.white;
    public Color hoverColor = new Color(1, 1, 1, 0.8f);

    [Header("Scene Settings")]
    public string gameSceneName = "SampleScene";
    public int gameSceneBuildIndex = 1;

    private List<Button> buttonComponents = new List<Button>();
    private bool buttonsActive = false;
    private bool isTransitioning = false;

    void Start()
    {
        if (bgmSource == null) bgmSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();

        fadeOverlay.color = Color.black;
        fadeOverlay.gameObject.SetActive(true);

        titleText.alpha = 0;

        foreach (TextMeshProUGUI buttonText in menuButtons)
        {
            buttonText.alpha = 0;
            buttonText.color = new Color(normalColor.r, normalColor.g, normalColor.b, 0);

            Button btn = buttonText.gameObject.GetComponent<Button>();
            if (btn == null) btn = buttonText.gameObject.AddComponent<Button>();

            btn.transition = Selectable.Transition.None;
            btn.interactable = false;

            buttonComponents.Add(btn);
        }

        StartCoroutine(CinematicIntro());
    }

    void Update()
    {
        if (buttonsActive && !isTransitioning && Input.GetKeyDown(KeyCode.Return))
        {
            OnButtonClicked("start");
        }
    }

    IEnumerator CinematicIntro()
    {
        // 1. fade from black to background
        float timer = 0;
        while (timer < initialFadeInTime)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, timer / initialFadeInTime);
            fadeOverlay.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        fadeOverlay.gameObject.SetActive(false);

        // 2. start background music
        if (backgroundMusic != null && bgmSource != null)
        {
            bgmSource.clip = backgroundMusic;
            bgmSource.loop = true;
            bgmSource.Play();
        }
        yield return new WaitForSeconds(titleAppearDelay);

        timer = 0;
        while (timer < titleFadeTime)
        {
            timer += Time.deltaTime;
            titleText.alpha = Mathf.Lerp(0, 1, timer / titleFadeTime);
            yield return null;
        }

        // 3. wait AFTER title is fully visible
        yield return new WaitForSeconds(buttonsAppearDelay);

        // 4. fade in buttons one by one
        for (int i = 0; i < menuButtons.Count; i++)
        {
            yield return StartCoroutine(FadeInButton(menuButtons[i], buttonFadeTime));

            if (i < menuButtons.Count - 1)
            {
                yield return new WaitForSeconds(timeBetweenButtons);
            }
        }
        ActivateButtons();
    }

    IEnumerator FadeInButton(TextMeshProUGUI buttonText, float duration)
    {
        float timer = 0;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            buttonText.alpha = Mathf.Lerp(0, 1, t);
            Color targetColor = normalColor;
            Color currentColor = buttonText.color;
            currentColor.r = Mathf.Lerp(0, targetColor.r, t);
            currentColor.g = Mathf.Lerp(0, targetColor.g, t);
            currentColor.b = Mathf.Lerp(0, targetColor.b, t);
            currentColor.a = buttonText.alpha;
            buttonText.color = currentColor;

            yield return null;
        }

        buttonText.alpha = 1;
        buttonText.color = normalColor;
    }

    void ActivateButtons()
    {
        buttonsActive = true;

        for (int i = 0; i < menuButtons.Count; i++)
        {
            TextMeshProUGUI buttonText = menuButtons[i];
            Button btn = buttonComponents[i];

            btn.interactable = true;

            string buttonName = buttonText.text.ToLower().Trim();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnButtonClicked(buttonName));
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isTransitioning) return;

        foreach (TextMeshProUGUI buttonText in menuButtons)
        {
            if (eventData.pointerEnter == buttonText.gameObject)
            {
                buttonText.color = hoverColor;

                if (buttonHoverSound != null && sfxSource != null)
                    sfxSource.PlayOneShot(buttonHoverSound);

                break;
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isTransitioning) return;

        foreach (TextMeshProUGUI buttonText in menuButtons)
        {
            buttonText.color = normalColor;
        }
    }

    void OnButtonClicked(string buttonName)
    {
        if (isTransitioning) return;

        isTransitioning = true;

        if (buttonClickSound != null && sfxSource != null)
            sfxSource.PlayOneShot(buttonClickSound);

        StartCoroutine(FadeOutAndLoadScene(buttonName));
    }

    IEnumerator FadeOutAndLoadScene(string buttonName)
    {
        foreach (Button btn in buttonComponents)
        {
            btn.interactable = false;
        }

        fadeOverlay.gameObject.SetActive(true);

        float timer = 0;
        while (timer < sceneTransitionFadeTime)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, timer / sceneTransitionFadeTime);
            fadeOverlay.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        if (bgmSource != null && bgmSource.isPlaying)
        {
            float startVolume = bgmSource.volume;
            float fadeTimer = 0;
            float fadeDuration = 0.5f;

            while (fadeTimer < fadeDuration)
            {
                fadeTimer += Time.deltaTime;
                bgmSource.volume = Mathf.Lerp(startVolume, 0, fadeTimer / fadeDuration);
                yield return null;
            }
            bgmSource.Stop();
            bgmSource.volume = startVolume;
        }

        switch (buttonName)
        {
            case "start":
            case "play":
                LoadGameScene();
                break;

            case "quit":
            case "exit":
                QuitGame();
                break;
        }
    }

    void LoadGameScene()
    {
        if (gameSceneBuildIndex >= 0 && gameSceneBuildIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(gameSceneBuildIndex);
        }
        else if (!string.IsNullOrEmpty(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextSceneIndex);
            }
        }
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}