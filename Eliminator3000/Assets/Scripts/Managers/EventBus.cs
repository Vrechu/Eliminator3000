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
    public PlayerJoinedEvent(int cPlayer)
    { PlayerProfile = cPlayer; }
}

public class AllPlayersDeadEvent : Event { }

public class PlayerSpawnEvent : Event
{
    public int Player;
    public Transform SpawnPoint;
    public PlayerSpawnEvent(int cPlayer, Transform cSpawnPoint)
    {
        Player = cPlayer;
        SpawnPoint = cSpawnPoint;
    }
}

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
public class LevelExitEvent : Event { }
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

public class  LevelEnteredEvent : Event
{
    public int LevelNumber;
    public LevelEnteredEvent(int cLevelNumber)
    {
        LevelNumber = cLevelNumber;
    }
}

public class FinishedLevelEvent : Event { }

#endregion

#region Player combat events

public class PlayerGunPickupEvent : Event
{
    public int Player;
    public PlayerGunPickupEvent(int cPlayer)
    {
        Player = cPlayer;
    }
}

#endregion