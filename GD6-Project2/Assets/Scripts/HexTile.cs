using UnityEngine;
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
