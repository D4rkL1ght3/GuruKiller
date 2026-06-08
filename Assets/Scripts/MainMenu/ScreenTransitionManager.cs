using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    [Header("Transition Settings")]
    public float dissolveDuration = 1f;

    private static SceneTransitionManager instance;
    private static GameObject persistentFadePanel;
    private static Canvas persistentCanvas;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            if (instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }
    }

    public static void LoadSceneWithDissolve(string sceneName, float duration = -1f)
    {
        if (instance != null)
        {
            float dur = duration > 0 ? duration : instance.dissolveDuration;
            instance.StartCoroutine(instance.DissolveAndLoad(sceneName, dur));
        }
        else
        {
            Debug.LogWarning("SceneTransitionManager not found, loading scene directly");
            SceneManager.LoadScene(sceneName);
        }
    }

    IEnumerator DissolveAndLoad(string sceneName, float duration)
    {
        CreatePersistentFadePanel();

        if (persistentFadePanel != null)
        {
            Image fadeImage = persistentFadePanel.GetComponent<Image>();
            GraphicRaycaster raycaster = persistentCanvas.GetComponent<GraphicRaycaster>();
            raycaster.enabled = false; 

            Color startColor = new Color(0, 0, 0, 0);
            Color endColor = Color.black; 

            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                float progress = t / duration;
                fadeImage.color = Color.Lerp(startColor, endColor, progress);
                yield return null;
            }
            fadeImage.color = endColor; 
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame(); 

        CreatePersistentFadePanel();

        if (persistentFadePanel != null)
        {
            Image fadeImage = persistentFadePanel.GetComponent<Image>();
            GraphicRaycaster raycaster = persistentCanvas.GetComponent<GraphicRaycaster>();
            raycaster.enabled = false; 

            Color startColor = Color.black; 
            Color endColor = new Color(0, 0, 0, 0); 

            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                float progress = t / duration;
                fadeImage.color = Color.Lerp(startColor, endColor, progress);
                yield return null;
            }
            fadeImage.color = endColor; 
            raycaster.enabled = true;
        }
    }

    void CreatePersistentFadePanel()
    {
        if (persistentFadePanel != null)
        {
            DestroyImmediate(persistentFadePanel);
        }

        GameObject canvasObj = new GameObject("PersistentTransitionCanvas");
        persistentCanvas = canvasObj.AddComponent<Canvas>();
        persistentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        persistentCanvas.sortingOrder = 9999; 

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        GraphicRaycaster raycaster = canvasObj.AddComponent<GraphicRaycaster>();
        raycaster.enabled = true; 

        persistentFadePanel = new GameObject("PersistentFadePanel");
        persistentFadePanel.transform.SetParent(canvasObj.transform);

        Image fadeImage = persistentFadePanel.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.raycastTarget = false; 

        RectTransform rectTransform = persistentFadePanel.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        DontDestroyOnLoad(canvasObj);
    }

    public static bool InstanceExists()
    {
        return instance != null;
    }
}