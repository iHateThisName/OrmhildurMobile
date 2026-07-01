using Gaskellgames;
using System;
using UnityEngine;

public abstract class MinigameBase : MonoBehaviour
{
    public float TimeLimit = 5f;
    public Action<bool> OnMinigameComplete;

    protected float timeRemaining;
    protected bool isPlaying = false;
    public bool isTimeOutWinConditon = false;

    [Button]
    public virtual void StartMinigame()
    {
        timeRemaining = TimeLimit;
        isPlaying = true;
    }

    protected virtual void Update()
    {
        if (!isPlaying) return;

        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0)
        {
            if (isTimeOutWinConditon)
            {
                EndMinigame(true); // Time out, but player won
            }
            else
            {
                EndMinigame(false); // Time out!
            }
        }
    }

    protected virtual void EndMinigame(bool wasVictorious)
    {
        isPlaying = false;
        OnMinigameComplete?.Invoke(wasVictorious);
    }
}