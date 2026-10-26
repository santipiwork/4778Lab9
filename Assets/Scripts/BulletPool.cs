using UnityEngine;

// Object Pool: create bullets once, then activate and return the same objects.
public class BulletPool
{
    private readonly Bullet[] bullets;
    public int Capacity => bullets.Length;
    public int ActiveCount { get; private set; }
    public int Ammo { get; private set; }

    public BulletPool(Sprite sprite, int capacity, Transform parent)
    {
        bullets = new Bullet[capacity];
        Ammo = capacity;
        for (int i = 0; i < capacity; i++)
        {
            GameObject obj = new GameObject("Bullet " + (i + 1));
            obj.transform.SetParent(parent);
            obj.AddComponent<SpriteRenderer>().sprite = sprite;
            bullets[i] = obj.AddComponent<Bullet>();
            bullets[i].Pool = this;
            obj.SetActive(false);
        }
    }

    public bool Fire(Vector3 position)
    {
        if (Ammo == 0) return false;
        foreach (Bullet bullet in bullets)
        {
            if (bullet.gameObject.activeSelf) continue;
            bullet.transform.position = position;
            bullet.gameObject.SetActive(true);
            ActiveCount++;
            Ammo--;
            return true;
        }
        return false; // Fixed capacity: never allocate extra bullets during play.
    }

    public void Return(Bullet bullet, bool hitEnemy = false)
    {
        if (!bullet.gameObject.activeSelf) return;
        bullet.gameObject.SetActive(false);
        ActiveCount--;
        // Recycling an object does not reload ammunition unless it hit an enemy.
        if (hitEnemy) Ammo = Capacity;
    }

    public void Reset()
    {
        foreach (Bullet bullet in bullets) Return(bullet);
        Ammo = Capacity;
    }
}
