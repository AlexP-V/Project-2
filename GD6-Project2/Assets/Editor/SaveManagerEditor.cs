using UnityEditor;
using UnityEngine;

public static class SaveManagerEditor
{
    [MenuItem("Tools/Clear Runs Data")]
    private static void ClearRunsMenu()
    {
        SaveManager.ClearAllRuns();
        Debug.Log("Runs data cleared via Tools/Clear Runs Data.");
    }
}
