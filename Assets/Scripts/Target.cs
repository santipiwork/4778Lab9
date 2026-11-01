using System;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public static readonly List<Target> ActiveTargets = new List<Target>();
    public event Action<int> WasHit;
    public float Speed { get; set; }
    public int Points { get; set; }
    public float HalfWidth { get; set; }
    private float direction = 1f;
    private Vector3 startPosition;

    private void Awake() { ActiveTargets.Add(this); }
    private void Start() { startPosition = transform.position; }
    private void OnDestroy() { ActiveTargets.Remove(this); }

    private void Update()
    {
        Vector3 position = transform.position;
        position.x += direction * Speed * Time.deltaTime;
        float edge = Mathf.Max(0, Camera.main.orthographicSize * Camera.main.aspect - HalfWidth);
        if (Mathf.Abs(position.x) >= edge)
        {
            position.x = Mathf.Clamp(position.x, -edge, edge);
            direction *= -1f;
        }
        transform.position = position;
    }

    public void Hit()
    {
        if (!gameObject.activeSelf) return;
        gameObject.SetActive(false);
        WasHit?.Invoke(Points); // Notify observers; the target never changes the score itself.
        Invoke(nameof(Respawn), 1f);
    }

    private void Respawn() { gameObject.SetActive(true); }

    public void RestoreVisibility(bool active)
    {
        // Remove an old respawn timer before restoring the saved state.
        CancelInvoke();
        gameObject.SetActive(active);
        if (!active) Invoke(nameof(Respawn), 1f);
    }

    public void ResetTarget()
    {
        CancelInvoke();
        transform.position = startPosition;
        direction = 1f;
        gameObject.SetActive(true);
    }
}
