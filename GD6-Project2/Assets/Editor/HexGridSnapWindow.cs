using UnityEngine;
using UnityEditor;

public class HexGridSnapWindow : EditorWindow
{
    private float hexRadius = 1f;
    private bool preserveZ = true;

    [MenuItem("Tools/Hex Grid Snap")]
    public static void ShowWindow()
    {
        GetWindow<HexGridSnapWindow>("Hex Grid Snap");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Flat-top Hex Grid Snap", EditorStyles.boldLabel);
        hexRadius = EditorGUILayout.FloatField("Hex Radius", hexRadius);
        preserveZ = EditorGUILayout.Toggle("Preserve Z", preserveZ);

        EditorGUILayout.Space();

        if (GUILayout.Button("Snap Selected"))
        {
            SnapSelected();
        }

        if (GUILayout.Button("Snap All Children of Selected"))
        {
            SnapChildrenOfSelected();
        }

        if (GUILayout.Button("Auto-detect Hex Radius (from Selected)"))
        {
            AutoDetectRadiusFromSelection();
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Select tile GameObjects in the Hierarchy then click a button. Uses flat-top axial coordinates.", MessageType.Info);
    }

    private void SnapSelected()
    {
        var objs = Selection.gameObjects;
        if (objs == null || objs.Length == 0)
        {
            EditorUtility.DisplayDialog("Hex Grid Snap", "No GameObjects selected.", "OK");
            return;
        }

        int count = 0;
        foreach (var go in objs)
        {
            if (go == null) continue;
            SnapTransform(go.transform);
            count++;
        }

        EditorUtility.DisplayDialog("Hex Grid Snap", $"Snapped {count} object(s).", "OK");
    }

    private void SnapChildrenOfSelected()
    {
        var objs = Selection.gameObjects;
        if (objs == null || objs.Length == 0)
        {
            EditorUtility.DisplayDialog("Hex Grid Snap", "No GameObjects selected.", "OK");
            return;
        }

        int count = 0;
        foreach (var go in objs)
        {
            if (go == null) continue;
            foreach (Transform t in go.GetComponentsInChildren<Transform>())
            {
                if (t == null) continue;
                SnapTransform(t);
                count++;
            }
        }

        EditorUtility.DisplayDialog("Hex Grid Snap", $"Snapped {count} transform(s).", "OK");
    }

    private void AutoDetectRadiusFromSelection()
    {
        var objs = Selection.gameObjects;
        if (objs == null || objs.Length == 0)
        {
            EditorUtility.DisplayDialog("Auto-detect Hex Radius", "No GameObjects selected. Please select the tile GameObjects or a parent containing them.", "OK");
            return;
        }

        // Collect positions from selection (or their children)
        var positions = new System.Collections.Generic.List<Vector2>();
        foreach (var go in objs)
        {
            if (go == null) continue;
            // If selection is a parent with many children, include children
            var children = go.GetComponentsInChildren<Transform>(includeInactive: false);
            if (children != null && children.Length > 1)
            {
                foreach (var t in children)
                {
                    if (t == null) continue;
                    // ignore the parent itself if it's the root
                    positions.Add(new Vector2(t.position.x, t.position.y));
                }
            }
            else
            {
                positions.Add(new Vector2(go.transform.position.x, go.transform.position.y));
            }
        }

        // Need at least two positions
        if (positions.Count < 2)
        {
            EditorUtility.DisplayDialog("Auto-detect Hex Radius", "Need at least two tile positions to detect spacing.", "OK");
            return;
        }

        // For each point compute nearest-neighbor distance (non-zero)
        var nearest = new System.Collections.Generic.List<float>();
        for (int i = 0; i < positions.Count; i++)
        {
            float min = float.MaxValue;
            for (int j = 0; j < positions.Count; j++)
            {
                if (i == j) continue;
                float d = Vector2.Distance(positions[i], positions[j]);
                if (d > 0f && d < min) min = d;
            }
            if (min < float.MaxValue) nearest.Add(min);
        }

        if (nearest.Count == 0)
        {
            EditorUtility.DisplayDialog("Auto-detect Hex Radius", "Could not compute nearest-neighbor distances.", "OK");
            return;
        }

        // Compute median of nearest distances to reduce outliers
        nearest.Sort();
        float median = nearest[nearest.Count / 2];

        // Filter distances around median (0.5x - 1.5x) and compute average
        double sum = 0.0;
        int count = 0;
        for (int i = 0; i < nearest.Count; i++)
        {
            float v = nearest[i];
            if (v >= 0.5f * median && v <= 1.5f * median)
            {
                sum += v;
                count++;
            }
        }
        float avgNearest = (count > 0) ? (float)(sum / count) : median;

        // For flat-top hex grid neighbor center distance = size * sqrt(3)
        float detectedSize = avgNearest / Mathf.Sqrt(3f);

        hexRadius = detectedSize;
        Repaint();

        // Apply detected radius to any PenguinController instances in the scene
        var penguins = UnityEngine.Object.FindObjectsOfType<global::PenguinController>();
        int applied = 0;
        if (penguins != null && penguins.Length > 0)
        {
            foreach (var p in penguins)
            {
                if (p == null) continue;
                Undo.RecordObject(p, "Auto-detect Hex Radius");
                p.hexRadius = detectedSize;
                EditorUtility.SetDirty(p);
                applied++;
            }
        }

        EditorUtility.DisplayDialog("Auto-detect Hex Radius", $"Detected hex radius: {detectedSize:F4} (avg neighbor center distance {avgNearest:F4})\nApplied to {applied} PenguinController(s)", "OK");
    }

    private void SnapTransform(Transform tr)
    {
        Vector3 pos = tr.position;
        Vector2 axialF = HexGridUtility.WorldToAxial(new Vector2(pos.x, pos.y), hexRadius);
        int q, r;
        HexGridUtility.AxialRound(axialF.x, axialF.y, out q, out r);
        Vector2 world = HexGridUtility.AxialToWorld(q, r, hexRadius);

        Undo.RecordObject(tr, "Hex Grid Snap");
        tr.position = new Vector3(world.x, world.y, preserveZ ? pos.z : 0f);
        EditorUtility.SetDirty(tr);
    }
}
