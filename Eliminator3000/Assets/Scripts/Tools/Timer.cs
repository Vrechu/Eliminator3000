using UnityEngine;

public class Timer
{
    public float Duration { get; private set; }
    public float TimeLeft { get; private set; }
    public bool Loop { get; private set; }
    public bool Paused;

    public Timer(float cDuration, bool cLoop = false, bool cPaused = false)
    {
        Duration = cDuration;
        TimeLeft = cDuration;
        this.Loop = cLoop;
        Paused = cPaused;
    }

    public bool IsFinished()
    {
        if (Paused) return false;
        if (TimeLeft > 0)
        {
            TimeLeft -= UnityEngine.Time.deltaTime;
            if (TimeLeft < 0) TimeLeft = 0;
            return false;
        }
        if (Loop) TimeLeft = Duration;
        return true;
    }
}
