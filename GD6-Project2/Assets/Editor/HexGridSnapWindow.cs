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
