using UnityEngine;

// Builder: assemble a target step by step with different configurations.
public class TargetBuilder
{
    private readonly Sprite sprite;
    private float speed = 1f;
    private float size = 1f;
    private int points = 10;

    public TargetBuilder(Sprite sprite) { this.sprite = sprite; }
    public TargetBuilder WithSpeed(float value) { speed = value; return this; }
    public TargetBuilder WithSize(float value) { size = value; return this; }
    public TargetBuilder WithPoints(int value) { points = value; return this; }

    public Target Build(string name, Vector3 position, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);
        obj.transform.position = position;
        obj.transform.localScale = Vector3.one * size;
        SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        Target target = obj.AddComponent<Target>();
        target.Speed = speed;
        target.Points = points;
        target.HalfWidth = renderer.bounds.extents.x;
        return target;
    }
}
