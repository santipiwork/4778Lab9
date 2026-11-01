// Same contract as the slides, using typed data instead of LitJSON.
public interface ISaveable
{
    string SaveID { get; }
    TransformSaveData SavedData { get; }
    void LoadFromData(TransformSaveData data);
}
