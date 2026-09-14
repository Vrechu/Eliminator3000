using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class Event { }

public class EventBus<T> where T : Event
{
    public static event Action<T> OnEvent;

    public static void Subscribe(Action<T> method)
    {
        OnEvent += method;
    }

    public static void UnSubscribe(Action<T> method)
    {
        OnEvent -= method;
    }

    public static void Publish(T pEvent)
    {
        OnEvent?.Invoke(pEvent);
    }    
}


public class PlayerScoredEvent : Event
{
    public int Player;
    public int Score;
    public PlayerScoredEvent(int cPlayer, int cScore)
    {
        Player = cPlayer;
        Score = cScore;
    }
}

public class ScoreChangedEvent : Event
{
    public int Player;
    public int Score;
    public ScoreChangedEvent(int cPlayer, int cScore)
    {
        Player = cPlayer;
        Score = cScore;
    }
}