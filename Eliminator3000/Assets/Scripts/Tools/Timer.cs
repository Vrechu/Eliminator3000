using UnityEngine;

/// <summary>
/// Tool class for managing timers, allowing for duration tracking, looping, and pausing functionality.
/// </summary>
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

    /// <summary>
    /// Checks if the timer has finished. If not paused, it decrements the time left by the delta time. 
    /// If the timer reaches zero and is set to loop, it resets the time left to the duration. 
    /// Returns true if the timer has finished, false otherwise.
    /// </summary>
    /// <returns>True if the timer has finished, false otherwise.</returns>
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

    /// <summary>
    /// Resets the timer to its original duration or a new specified duration. 
    /// If a new duration is provided, it updates the timer's duration and resets the time left accordingly.
    /// </summary>
    /// <param name="_newDuration">New duration for the timer. If not provided, the timer resets to its original duration.</param>
    public void Reset(float _newDuration = -1)
    {
        if (_newDuration >= 0) Duration = _newDuration;
        TimeLeft = Duration;
    }
}
