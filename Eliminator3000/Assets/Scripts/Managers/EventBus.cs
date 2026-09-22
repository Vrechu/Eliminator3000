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


public class LoadSceneEvent : Event
{
    public string SceneName;
    public LoadSceneEvent(string cSceneName)
    {
        SceneName = cSceneName;
    }
}

#region Profile events

public class PlayerJoinedEvent : Event
{
    public int PlayerProfile;
    public PlayerJoinedEvent(int cPlayerProfile)
    { PlayerProfile = cPlayerProfile; }
}

public class AllPlayersDeadEvent : Event { }

#endregion

#region Main menu events

public class MainMenuStartGameEvent : Event { }

#endregion

#region Game state events
public class GameStateChangedEvent : Event
{
    public string NewState;
    public GameStateChangedEvent(string cNewState)
    {
        NewState = cNewState;
    }
}
public class GameStartEvent : Event { }
public class GamePauseEvent : Event { }
public class GameResumeEvent : Event { }
public class GameLoseEvent : Event { }
public class GameWinEvent : Event { }
#endregion

#region Player health events

public class PlayerLivesChangedEvent : Event
{
    public int Player;
    public int Lives;
    public PlayerLivesChangedEvent(int cPlayer, int cLives)
    {
        Player = cPlayer;
        Lives = cLives;
    }
}

public class PlayerHitEvent : Event
{
    public int Player;
    public int Damage;
    public PlayerHitEvent(int cPlayer, int cDamage)
    {
        Player = cPlayer;
        Damage = cDamage;
    }
}

public class  PlayerLivesAtZeroEvent : Event
{
    public int Player;
    public PlayerLivesAtZeroEvent(int cPlayer)
    {
        Player = cPlayer;
    }
}

#endregion

#region Player score events

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

#endregion

#region Level events

public class FinishedLevelEvent : Event { }

#endregion