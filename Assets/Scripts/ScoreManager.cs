using System.Collections.Generic;
using UnityEngine;

// Observer: subscribe to target events instead of targets knowing about the scoreboard.
public class ScoreManager : MonoBehaviour
{
    public int Score { get; private set; }
    private readonly List<Target> observed = new List<Target>();

    public void Observe(Target target)
    {
        if (observed.Contains(target)) return;
        observed.Add(target);
        target.WasHit += AddPoints;
    }

    private void AddPoints(int points) { Score += points; }
    public void ResetScore() { Score = 0; }
    public void RestoreScore(int savedScore) { Score = savedScore; }

    private void OnDestroy()
    {
        foreach (Target target in observed)
            if (target != null) target.WasHit -= AddPoints;
    }
}
