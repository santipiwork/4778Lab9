using System;
using System.IO;
using UnityEditor;
using UnityEngine;

// Run from Tools > Check Save and Load, or Unity's -executeMethod option.
public static class SaveLoadChecks
{
    [MenuItem("Tools/Check Save and Load")]
    public static void Run()
    {
        string folder = Path.Combine(Path.GetTempPath(), "CarnivalSaveCheck-" + Guid.NewGuid().ToString("N"));
        var root = new GameObject("Save test (temporary)");
        try
        {
            var score = root.AddComponent<ScoreManager>();
            var savers = new ISaveable[4];
            for (int i = 0; i < savers.Length; i++)
            {
                var obj = new GameObject("Test object " + i);
                obj.transform.SetParent(root.transform);
                var saver = obj.AddComponent<TransformSaver>();
                saver.Initialize("object-" + i);
                obj.transform.position = new Vector3(i, i * 2, 0);
                savers[i] = saver;
            }
            var service = new SavingService(savers, score, folder);
            Require(!service.LoadGame(out _), "Missing save should fail without changing the game.");
            score.RestoreScore(70);
            ((TransformSaver)savers[3]).gameObject.SetActive(false);
            Require(service.SaveGame(out _), "Save should succeed.");
            Require(File.ReadAllBytes(service.ScorePath).Length == 4, "Score must be a binary Int32.");
            Require(JsonUtility.FromJson<PositionSaveFile>(File.ReadAllText(service.PositionsPath)).objects.Length == 4,
                "JSON must include player and all three enemies, even inactive ones.");

            // Recreate objects, then reverse registration order to test persistent IDs.
            for (int i = 0; i < savers.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(((TransformSaver)savers[i]).gameObject);
                var obj = new GameObject("Replacement " + i);
                obj.transform.SetParent(root.transform);
                var saver = obj.AddComponent<TransformSaver>();
                saver.Initialize("object-" + i);
                savers[i] = saver;
            }
            Array.Reverse(savers);
            service = new SavingService(savers, score, folder);
            score.ResetScore();
            Require(service.LoadGame(out _), "Load after object recreation should succeed.");
            Require(score.Score == 70, "Score must be restored.");
            for (int i = 0; i < savers.Length; i++)
            {
                int original = 3 - i;
                var saver = (TransformSaver)savers[i];
                Require(saver.transform.position == new Vector3(original, original * 2, 0), "Position must match its ID.");
                Require(saver.gameObject.activeSelf == (original != 3), "Visibility must be restored.");
            }

            score.RestoreScore(90);
            Require(service.SaveGame(out _), "Overwriting the save should succeed.");
            score.RestoreScore(100);
            File.WriteAllText(service.PositionsPath, "{}");
            Require(!service.LoadGame(out _) && score.Score == 100, "Invalid JSON data must not change the score.");
            Require(service.SaveGame(out _), "Save should replace invalid data.");
            File.WriteAllBytes(service.ScorePath, new byte[] { 1 });
            Require(!service.LoadGame(out _) && score.Score == 100, "Truncated binary must not change the score.");
            Debug.Log("SAVE/LOAD CHECKS PASSED: missing files, binary/JSON formats, all positions, stable IDs, visibility, overwrite, and invalid data.");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
            File.Delete(Path.Combine(folder, "score.bin"));
            File.Delete(Path.Combine(folder, "positions.json"));
            if (Directory.Exists(folder)) Directory.Delete(folder);
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new Exception("Save/load check failed: " + message);
    }
}
