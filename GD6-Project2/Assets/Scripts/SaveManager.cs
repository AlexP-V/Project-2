using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class StepEntry
{
    public int q;
    public int r;
    public long t; // unix ms
}

[Serializable]
public class FakeTrailEntry
{
    public int q;
    public int r;
    public long t;
}

[Serializable]
public class FootprintEntry
{
    public float posX;
    public float posY;
    public float posZ;
    public float rotZ;
    public float scaleX = 1f;
    public float scaleY = 1f;
    public float colorR = 1f;
    public float colorG = 1f;
    public float colorB = 1f;
    public float colorA = 1f;
    public bool isFake = false;
    public long t;
}

[Serializable]
public class RunData
{
    public string id;
    public int startQ;
    public int startR;
    public long startTime;
    public long endTime;
    public string result; // "finished" | "gave_up" | "trap" | null
    public int stepsCount;
    public List<StepEntry> steps = new List<StepEntry>();
    public List<FakeTrailEntry> fakeTrails = new List<FakeTrailEntry>();
    public List<FootprintEntry> footprints = new List<FootprintEntry>();
}

[Serializable]
public class RunsFile
{
    public List<RunData> runs = new List<RunData>();
}

public static class SaveManager
{
    private static string fileName = "runs.json";
    private static string tmpFileName = "runs.json.tmp";
    private static RunsFile cache = null;
    private static RunData currentRun = null;

    private static string FilePath()
    {
        return Path.Combine(Application.persistentDataPath, fileName);
    }

    private static string TmpPath()
    {
        return Path.Combine(Application.persistentDataPath, tmpFileName);
    }

    private static void EnsureCacheLoaded()
    {
        if (cache != null) return;
        string path = FilePath();
        cache = new RunsFile();
        try
        {
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                var rf = JsonUtility.FromJson<RunsFile>(json);
                if (rf != null && rf.runs != null) cache = rf;
                // ensure lists are non-null for older files
                if (cache != null)
                {
                    foreach (var r in cache.runs)
                    {
                        if (r.steps == null) r.steps = new List<StepEntry>();
                        if (r.fakeTrails == null) r.fakeTrails = new List<FakeTrailEntry>();
                        if (r.footprints == null) r.footprints = new List<FootprintEntry>();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("SaveManager: failed to load runs file: " + ex.Message);
            cache = new RunsFile();
        }
    }

    private static void WriteCacheToDisk()
    {
        try
        {
            string tmp = TmpPath();
            string final = FilePath();
            string json = JsonUtility.ToJson(cache, true);
            Directory.CreateDirectory(Path.GetDirectoryName(final));
            File.WriteAllText(tmp, json);
            // replace
            if (File.Exists(final)) File.Delete(final);
            File.Move(tmp, final);
        }
        catch (Exception ex)
        {
            Debug.LogError("SaveManager: failed to write runs file: " + ex.Message);
        }
    }

    public static void StartRun(int startQ, int startR)
    {
        EnsureCacheLoaded();
        currentRun = new RunData();
        currentRun.id = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8);
        currentRun.startQ = startQ;
        currentRun.startR = startR;
        currentRun.startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        currentRun.result = null;
    }

    public static void RecordStep(int q, int r, long unixMs)
    {
        if (currentRun == null) return;
        currentRun.steps.Add(new StepEntry { q = q, r = r, t = unixMs });
        currentRun.stepsCount = currentRun.steps.Count;
    }

    public static void RecordFakeTrail(int q, int r, long unixMs)
    {
        if (currentRun == null) return;
        currentRun.fakeTrails.Add(new FakeTrailEntry { q = q, r = r, t = unixMs });
    }

    public static void RecordFootprint(FootprintEntry fp)
    {
        if (currentRun == null) return;
        if (fp == null) return;
        currentRun.footprints.Add(fp);
    }

    public static void FinalizeRun(string result)
    {
        if (currentRun == null)
        {
            EnsureCacheLoaded();
            return;
        }
        currentRun.result = result;
        currentRun.endTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        currentRun.stepsCount = currentRun.steps.Count;
        EnsureCacheLoaded();
        cache.runs.Add(currentRun);
        WriteCacheToDisk();
        currentRun = null;
    }

    public static List<RunData> LoadRecentRuns(int maxRuns)
    {
        EnsureCacheLoaded();
        var list = cache.runs;
        int take = Mathf.Min(maxRuns, list.Count);
        var result = new List<RunData>();
        for (int i = list.Count - 1; i >= 0 && result.Count < take; i--)
        {
            result.Add(list[i]);
        }
        return result;
    }

    public static int? GetBestFinishedSteps()
    {
        EnsureCacheLoaded();
        int best = int.MaxValue;
        bool found = false;
        foreach (var r in cache.runs)
        {
            if (r.result == "finished")
            {
                found = true;
                if (r.stepsCount < best) best = r.stepsCount;
            }
        }
        if (!found) return null;
        return best;
    }

    public static void ClearAllRuns()
    {
        // clear in-memory cache and delete the file on disk
        try
        {
            cache = new RunsFile();
            string path = FilePath();
            if (File.Exists(path)) File.Delete(path);
            Debug.Log("SaveManager: cleared all runs (deleted) -> " + path);
        }
        catch (Exception ex)
        {
            Debug.LogError("SaveManager: failed to clear runs: " + ex.Message);
        }
    }
}
