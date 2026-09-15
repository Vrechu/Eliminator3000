using UnityEngine;

public class Timer
{
    public float Duration { get; private set; }
    public float Time { get; private set; }
    public bool Loop { get; private set; }

    public Timer(float cDuration, bool cLoop = false)
    {
        Duration = cDuration;
        Time = cDuration;
        this.Loop = cLoop;
    }

    public bool IsFinished()
    {
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
