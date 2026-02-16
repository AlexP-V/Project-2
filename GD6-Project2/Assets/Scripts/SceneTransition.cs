using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    [Tooltip("Default fade duration in seconds")]
    public float defaultDuration = 1f;
    [Tooltip("Color to fade to/from (alpha is controlled by the transition)")]
    public Color fadeColor = Color.white;

    private Image fadeImage;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupCanvasAndImage();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void SetupCanvasAndImage()
    {
        var canvas = gameObject.GetComponent<Canvas>();
        if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        var imgGO = new GameObject("FadeImage");
        imgGO.transform.SetParent(transform, false);
        fadeImage = imgGO.AddComponent<Image>();
        var rt = fadeImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
    }

    public void FadeToScene(int sceneBuildIndex, float duration)
    {
        StartCoroutine(FadeAndSwitch(sceneBuildIndex, Mathf.Max(0.01f, duration)));
    }

    public static void LoadSceneWithFade(int sceneBuildIndex, float duration = -1f)
    {
        if (duration <= 0f) duration = Instance != null ? Instance.defaultDuration : 1f;
        if (Instance == null)
        {
            var go = new GameObject("SceneTransition");
            Instance = go.AddComponent<SceneTransition>();
            Instance.defaultDuration = duration;
        }
        Instance.FadeToScene(sceneBuildIndex, duration);
    }

    IEnumerator FadeAndSwitch(int sceneBuildIndex, float duration)
    {
        if (fadeImage == null) SetupCanvasAndImage();

        float half = duration * 0.5f;
        yield return StartCoroutine(Fade(0f, 1f, half));

        var ao = SceneManager.LoadSceneAsync(sceneBuildIndex);
        while (!ao.isDone)
        {
            yield return null;
        }

        yield return StartCoroutine(Fade(1f, 0f, half));
    }

    IEnumerator Fade(float from, float to, float dur)
    {
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, to, Mathf.Clamp01(t / dur));
            var c = fadeImage.color;
            c.r = fadeColor.r;
            c.g = fadeColor.g;
            c.b = fadeColor.b;
            c.a = a;
            fadeImage.color = c;
            yield return null;
        }
        var cc = fadeImage.color;
        cc.r = fadeColor.r;
        cc.g = fadeColor.g;
        cc.b = fadeColor.b;
        cc.a = to;
        fadeImage.color = cc;
    }
}
