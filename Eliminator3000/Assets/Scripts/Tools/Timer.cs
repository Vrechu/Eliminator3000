using UnityEngine;

public class Timer
{
    public float Duration { get; private set; }
    public float Time { get; private set; }
    public bool Loop { get; private set; }
    public bool Paused;

    public Timer(float cDuration, bool cLoop = false, bool cPaused = false)
    {
        Duration = cDuration;
        Time = cDuration;
        this.Loop = cLoop;
        Paused = cPaused;
    }

    public bool IsFinished()
    {
        if (Paused) return false;
        if (Time > 0)
        {
            Time -= UnityEngine.Time.deltaTime;
            if (Time < 0) Time = 0;
            return false;
        }
        if (Loop) Time = Duration;
        return true;
    }
}
