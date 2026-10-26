using UnityEngine;

public class Bullet : MonoBehaviour
{
    public BulletPool Pool { get; set; }

    private void Update()
    {
        Vector3 previous = transform.position;
        transform.position += Vector3.up * (10f * Time.deltaTime);
        // Sweep the traveled distance so a bullet cannot skip a target on a slow frame.
        foreach (Target target in Target.ActiveTargets)
        {
            if (!target.gameObject.activeSelf) continue;
            Bounds bounds = target.GetComponent<SpriteRenderer>().bounds;
            bounds.Expand(new Vector3(0.12f, 0.2f, 0));
            if (previous.x >= bounds.min.x && previous.x <= bounds.max.x &&
                previous.y <= bounds.max.y && transform.position.y >= bounds.min.y)
            {
                target.Hit();
                Pool.Return(this, hitEnemy: true);
                return;
            }
        }
        if (transform.position.y > 5.5f) Pool.Return(this);
    }
}
