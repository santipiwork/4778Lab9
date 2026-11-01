using System;
using System.IO;
using UnityEngine;

[Serializable]
public class PositionSaveFile
{
    public TransformSaveData[] objects;
}

// One save slot: score.bin holds an integer; positions.json holds transforms.
public class SavingService
{
    private readonly ISaveable[] objects;
    private readonly ScoreManager score;
    private readonly string folder;
    public string ScorePath => Path.Combine(folder, "score.bin");
    public string PositionsPath => Path.Combine(folder, "positions.json");

    public SavingService(ISaveable[] objects, ScoreManager score, string folder)
    {
        this.objects = objects;
        this.score = score;
        this.folder = folder;
    }

    public bool SaveGame(out string message)
    {
        try
        {
            var data = new PositionSaveFile { objects = new TransformSaveData[objects.Length] };
            for (int i = 0; i < objects.Length; i++) data.objects[i] = objects[i].SavedData;
            Directory.CreateDirectory(folder);
            File.WriteAllText(PositionsPath, JsonUtility.ToJson(data, true));
            // BinaryWriter stores the score as four binary bytes, not text.
            using (var writer = new BinaryWriter(File.Create(ScorePath)))
                writer.Write(score.Score);
            message = "Game saved.";
            Debug.Log("Saved game to " + folder);
            return true;
        }
        catch (Exception error) when (error is IOException || error is UnauthorizedAccessException || error is ArgumentException)
        {
            message = "Could not save game.";
            Debug.LogWarning(message + " " + error.Message);
            return false;
        }
    }

    public bool LoadGame(out string message)
    {
        if (!File.Exists(ScorePath) || !File.Exists(PositionsPath))
        {
            message = "No complete save found. Press S to save first.";
            return false;
        }
        try
        {
            var data = JsonUtility.FromJson<PositionSaveFile>(File.ReadAllText(PositionsPath));
            int savedScore;
            using (var reader = new BinaryReader(File.OpenRead(ScorePath)))
            {
                if (reader.BaseStream.Length != sizeof(int)) throw new InvalidDataException("Invalid score file.");
                savedScore = reader.ReadInt32();
            }
            if (savedScore < 0 || data == null || data.objects == null || data.objects.Length != objects.Length)
                throw new InvalidDataException("Invalid save data.");

            // Check every record before changing anything in the running game.
            var matched = new TransformSaveData[objects.Length];
            for (int i = 0; i < objects.Length; i++)
            {
                foreach (var record in data.objects)
                {
                    if (record == null || record.saveID != objects[i].SaveID) continue;
                    if (matched[i] != null || !IsFinite(record.position))
                        throw new InvalidDataException("Invalid position record.");
                    matched[i] = record;
                }
                if (matched[i] == null) throw new InvalidDataException("Missing object: " + objects[i].SaveID);
            }
            for (int i = 0; i < objects.Length; i++) objects[i].LoadFromData(matched[i]);
            score.RestoreScore(savedScore);
            message = "Game loaded.";
            return true;
        }
        catch (Exception error) when (error is IOException || error is UnauthorizedAccessException || error is ArgumentException)
        {
            message = "Could not load save files.";
            Debug.LogWarning(message + " " + error.Message);
            return false;
        }
    }

    private static bool IsFinite(Vector3 value)
    {
        return !float.IsNaN(value.x) && !float.IsInfinity(value.x)
            && !float.IsNaN(value.y) && !float.IsInfinity(value.y)
            && !float.IsNaN(value.z) && !float.IsInfinity(value.z);
    }
}
