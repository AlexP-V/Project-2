using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
public class HexTile : MonoBehaviour
{
    [Header("Axial Coordinates")]
    public int q = 0;
    public int r = 0;
    [Tooltip("Hex radius used to compute axial coords from world position if needed")]
    public float hexRadius = 1f;
    [Header("Special")]
    [Tooltip("Mark this tile as the finish tile. When the player reaches it the level can end.")]
    public bool isFinish = false;
    [Tooltip("Mark this tile as the start tile. Player can spawn here if configured.")]
    public bool isStart = false;
    [Tooltip("Mark this tile as a trap. When the player lands here special trap behavior triggers.")]
    public bool isTrap = false;

    [Tooltip("Optional GameObject (child) to enable when this tile's trap is activated.")]
    public GameObject trapOverlay;

    // runtime cached trap overlay renderer and original colour for fade operations
    SpriteRenderer _trapOverlaySR = null;
    Color _trapOverlayOriginalColor = Color.white;
    Coroutine _trapFadeCoroutine = null;
    
    // shake configuration for trap appearance
    [Tooltip("Duration of the trap shake when it appears")] public float trapShakeDuration = 0.35f;
    [Tooltip("Maximum positional magnitude of trap shake in world units")] public float trapShakeMagnitude = 0.12f;
    Coroutine _trapShakeCoroutine = null;
    Vector3 _trapOverlayOriginalLocalPos = Vector3.zero;

    public void SetTrapVisual(bool on)
    {
        if (trapOverlay == null) return;

        // If there's a sprite renderer, prefer fading; otherwise just toggle active state.
        if (on)
        {
            // cancel any pending fade-out
            if (_trapFadeCoroutine != null)
            {
                StopCoroutine(_trapFadeCoroutine);
                _trapFadeCoroutine = null;
            }

            if (_trapOverlaySR != null)
            {
                // restore original colour/alpha before showing
                _trapOverlaySR.color = _trapOverlayOriginalColor;
            }

            trapOverlay.SetActive(true);
            // start shake effect when shown
            if (_trapShakeCoroutine != null) StopCoroutine(_trapShakeCoroutine);
            _trapShakeCoroutine = StartCoroutine(ShakeTrap(trapOverlay.transform, trapShakeDuration, trapShakeMagnitude));
            Debug.Log("trap is visible");
        }
        else
        {
            if (_trapOverlaySR != null)
            {
                // stop any shaking so fade starts from original position
                if (_trapShakeCoroutine != null) { StopCoroutine(_trapShakeCoroutine); _trapShakeCoroutine = null; }

                // start fade-out coroutine (non-blocking)
                if (_trapFadeCoroutine != null) StopCoroutine(_trapFadeCoroutine);
                _trapFadeCoroutine = StartCoroutine(FadeOutAndDisable(_trapOverlaySR, 0.5f));
            }
            else
            {
                trapOverlay.SetActive(false);
                Debug.Log("trap is invisible");
            }
        }
    }

    [Header("Highlight")]
    public Color highlightColor = Color.yellow;

    SpriteRenderer sr;
    Color originalColor;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;
        UpdateAxialFromPosition();
        HexTileRegistry.Register(this);

        // If no explicit trap overlay assigned, try to find a child named like "trap" and use it.
        if (trapOverlay == null)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var c = transform.GetChild(i);
                if (c == null) continue;
                var name = c.gameObject.name;
                if (string.IsNullOrEmpty(name)) continue;
                if (name.ToLower().Contains("trap"))
                {
                    trapOverlay = c.gameObject;
                    break;
                }
            }
        }

        if (trapOverlay != null)
        {
            // cache sprite renderer and original colour if present
            _trapOverlaySR = trapOverlay.GetComponent<SpriteRenderer>();
            if (_trapOverlaySR != null) _trapOverlayOriginalColor = _trapOverlaySR.color;

            // cache original local position so shake can restore it
            _trapOverlayOriginalLocalPos = trapOverlay.transform.localPosition;

            trapOverlay.SetActive(false);
        }
    }

    IEnumerator FadeOutAndDisable(SpriteRenderer sr, float duration)
    {
        if (sr == null) yield break;
        float startA = sr.color.a;
        float t = 0f;
        Color c = sr.color;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            c.a = Mathf.Lerp(startA, 0f, p);
            sr.color = c;
            yield return null;
        }

        if (trapOverlay != null) trapOverlay.SetActive(false);

        // restore original color so next show uses original alpha
        if (sr != null) sr.color = _trapOverlayOriginalColor;

        _trapFadeCoroutine = null;
        Debug.Log("trap is invisible");
    }

    IEnumerator ShakeTrap(Transform t, float duration, float magnitude)
    {
        if (t == null) yield break;
        float elapsed = 0f;
        Vector3 original = _trapOverlayOriginalLocalPos;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float pct = Mathf.Clamp01(elapsed / duration);
            float damper = 1f - pct; // ease out
            float x = (Random.value * 2f - 1f) * magnitude * damper;
            float y = (Random.value * 2f - 1f) * magnitude * damper;
            t.localPosition = original + new Vector3(x, y, 0f);
            yield return null;
        }
        t.localPosition = original;
        _trapShakeCoroutine = null;
    }

    void OnEnable()
    {
        HexTileRegistry.Register(this);
    }

    void OnDisable()
    {
        HexTileRegistry.Unregister(this);
    }

    void OnDestroy()
    {
        HexTileRegistry.Unregister(this);
    }

    public void UpdateAxialFromPosition()
    {
        if (hexRadius <= 0f) return;
        Vector2 axialF = HexGridUtility.WorldToAxial(new Vector2(transform.position.x, transform.position.y), hexRadius);
        int nq, nr;
        HexGridUtility.AxialRound(axialF.x, axialF.y, out nq, out nr);
        q = nq; r = nr;
    }

    public void SetHighlighted(bool on)
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;
        sr.color = on ? highlightColor : originalColor;
    }
}

public static class HexTileRegistry
{
    // simple string key "q_r" -> HexTile
    static Dictionary<string, HexTile> dict = new Dictionary<string, HexTile>();

    static string Key(int q, int r) => q + "_" + r;

    public static void Register(HexTile t)
    {
        if (t == null) return;
        var k = Key(t.q, t.r);
        dict[k] = t;
    }

    public static void Unregister(HexTile t)
    {
        if (t == null) return;
        var k = Key(t.q, t.r);
        if (dict.ContainsKey(k) && dict[k] == t) dict.Remove(k);
    }

    public static HexTile GetAt(int q, int r)
    {
        HexTile t;
        dict.TryGetValue(Key(q, r), out t);
        return t;
    }

    public static HexTile GetNearest(Vector2 worldPos, float maxDistance)
    {
        HexTile best = null;
        float bestDist = maxDistance;
        foreach (var kv in dict)
        {
            var t = kv.Value;
            if (t == null) continue;
            if (!t.gameObject.activeInHierarchy) continue;
            var sr = t.GetComponent<SpriteRenderer>();
            if (sr != null && !sr.enabled) continue;

            Vector2 tp = new Vector2(t.transform.position.x, t.transform.position.y);
            float d = Vector2.Distance(tp, worldPos);
            if (d < bestDist)
            {
                bestDist = d;
                best = t;
            }
        }
        return best;
    }

    public static void Clear() => dict.Clear();
}
