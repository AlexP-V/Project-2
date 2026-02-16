using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class TutorialTextController : MonoBehaviour
{
    public TextMeshProUGUI tutorialTMP;
    public CanvasGroup canvasGroup;
    public Vector2 offset = new Vector2(24f, -24f);
    public float fadeDuration = 0.35f;
    public float mouseMoveThreshold = 2f; // pixels

    private RectTransform rect;
    private Vector2 _lastMousePos;
    private bool _shown = false;
    private bool _hiding = false;

    private bool _playerMoved = false;
    private bool _playerFake = false;

    void Awake()
    {
        if (tutorialTMP == null)
        {
            Debug.LogWarning("TutorialTextController: tutorialTMP not assigned.");
        }
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        if (tutorialTMP != null) rect = tutorialTMP.GetComponent<RectTransform>();
        _lastMousePos = Input.mousePosition;
    }

    void OnEnable()
    {
        PenguinController.OnFirstMove += OnFirstMove;
        PenguinController.OnFirstFakeTrail += OnFirstFake;
    }

    void OnDisable()
    {
        PenguinController.OnFirstMove -= OnFirstMove;
        PenguinController.OnFirstFakeTrail -= OnFirstFake;
    }

    void Update()
    {
        // detect first mouse movement
        if (!_shown && !_hiding)
        {
            Vector2 cur = Input.mousePosition;
            if (Vector2.Distance(cur, _lastMousePos) >= mouseMoveThreshold)
            {
                Show();
            }
            _lastMousePos = cur;
        }

        // if visible, float around the mouse
        if (_shown && rect != null)
        {
            Vector2 mouse = Input.mousePosition;
            rect.position = mouse + offset;
        }

        // if both conditions met, hide
        if (_shown && !_hiding && _playerMoved && _playerFake)
        {
            Hide();
        }
    }

    private void OnFirstMove()
    {
        _playerMoved = true;
    }

    private void OnFirstFake()
    {
        _playerFake = true;
    }

    public void Show()
    {
        if (_shown || _hiding) return;
        _shown = true;
        StopAllCoroutines();
        StartCoroutine(FadeCanvasGroup(0f, 1f, fadeDuration));
    }

    public void Hide()
    {
        if (!_shown || _hiding) return;
        _hiding = true;
        StopAllCoroutines();
        StartCoroutine(FadeOutAndDisable());
    }

    private IEnumerator FadeOutAndDisable()
    {
        yield return FadeCanvasGroup(1f, 0f, fadeDuration);
        _shown = false;
        _hiding = false;
        // optional: destroy or disable component
        gameObject.SetActive(false);
    }

    private IEnumerator FadeCanvasGroup(float from, float to, float duration)
    {
        float t = 0f;
        canvasGroup.alpha = from;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}
