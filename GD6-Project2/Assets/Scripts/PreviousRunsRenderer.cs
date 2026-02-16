using System;
using System.Collections.Generic;
using UnityEngine;

public class PreviousRunsRenderer : MonoBehaviour
{
    public GameObject footprintPrefab;
    public int maxRuns = 5;
    public Color[] runColors;
    public float footprintJitter = 0.02f;

    void Start()
    {
        if (footprintPrefab == null) return;
        var runs = SaveManager.LoadRecentRuns(maxRuns);
        var prefabSR = footprintPrefab.GetComponent<SpriteRenderer>();
        Color prefabBaseColor = (prefabSR != null) ? prefabSR.color : Color.white;
        for (int ri = 0; ri < runs.Count; ri++)
        {
            var run = runs[ri];
            // render footprints exactly; fake footprints use the prefab base color to avoid session tint
            foreach (var fp in run.footprints)
            {
                Vector3 pos = new Vector3(fp.posX, fp.posY, fp.posZ);
                var inst = Instantiate(footprintPrefab, pos, Quaternion.identity);
                inst.transform.rotation = Quaternion.Euler(0f, 0f, fp.rotZ);
                inst.transform.localScale = new Vector3(fp.scaleX, fp.scaleY, 1f);
                var sr = inst.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    if (fp.isFake)
                    {
                        Color c = prefabBaseColor;
                        c.a = fp.colorA; // preserve saved alpha
                        sr.color = c;
                    }
                    else
                    {
                        Color c = new Color(fp.colorR, fp.colorG, fp.colorB, fp.colorA);
                        sr.color = c;
                    }
                }
            }
        }
    }
}
