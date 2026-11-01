using System;
using UnityEngine;

[Serializable]
public class TransformSaveData
{
    public string saveID;
    public Vector3 position;
    public bool active;
}

public class TransformSaver : MonoBehaviour, ISaveable
{
    public string SaveID { get; private set; }

    // Fixed IDs let saves work after closing and reopening the game.
    public void Initialize(string id) { SaveID = id; }

    public TransformSaveData SavedData => new TransformSaveData
    {
        saveID = SaveID,
        position = transform.position,
        active = gameObject.activeSelf
    };

    public void LoadFromData(TransformSaveData data)
    {
        transform.position = data.position;
        Target target = GetComponent<Target>();
        if (target != null) target.RestoreVisibility(data.active);
        else gameObject.SetActive(data.active);
    }
}
