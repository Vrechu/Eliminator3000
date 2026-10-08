using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class Event { }

/// <summary>
/// Event bus for publishing and subscribing to events of type T.
/// </summary>
/// <typeparam name="T">Event type</typeparam>
public class EventBus<T> where T : Event
{
    public static event Action<T> OnEvent;

    /// <summary>
    /// Subscribe to an event of type T.
    /// </summary>
    /// <param name="method">The method to be called when the event is published.</param>
    public static void Subscribe(Action<T> method)
    {
        OnEvent += method;
    }

    /// <summary>
    /// Unsubscribe from an event of type T.
    /// </summary>
    /// <param name="method">The method to be removed from the event subscription.</param>
    public static void UnSubscribe(Action<T> method)
    {
        OnEvent -= method;
    }

    /// <summary>
    /// Publish an event of type T to all subscribers.
    /// </summary>
    /// <param name="pEvent">The event to be published.</param>
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
    public int PlayerIndex;
    public PlayerJoinedEvent(int cPlayer)
    { PlayerIndex = cPlayer; }
}

public class BothPlayersDeadEvent : Event { }

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

public class MainMenuEnterEvent : Event { }


#endregion

#region Game state events
public class GameLoseEvent : Event { }
public class GameWinEvent : Event { }  
public class LevelExitEvent : Event { }
#endregion

#region Player health events

public class PlayerHealthChangedEvent : Event
{
    public int Player;
    public int NewHealth;
    public PlayerHealthChangedEvent(int cPlayer, int cNewHealth)
    {
        Player = cPlayer;
        NewHealth = cNewHealth;
    }
}

public class PlayerHitEvent : Event
{
    public int Player;
    public int Damage;
    public int ScoreLoss;
    public PlayerHitEvent(int cPlayer, int cDamage, int cScoreLoss)
    {
        Player = cPlayer;
        Damage = cDamage;
        ScoreLoss = cScoreLoss;
    }
}

public class HealthPickupEvent : Event
{
    public int Player;
    public int HealthAmount;
    public HealthPickupEvent(int cPlayer, int cHealthAmount)
    {
        Player = cPlayer;
        HealthAmount = cHealthAmount;
    }
}

public class PlayerLivesChangedEvent : Event
{
    public int Player;
    public int NewLives;
    public PlayerLivesChangedEvent(int cPlayer, int cNewLives)
    {
        Player = cPlayer;
        NewLives = cNewLives;
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

public class  PlayerAvatarInstantiatedEvent : Event
{
    public int PlayerIndex;
    public PlayerAvatarInstantiatedEvent(int cPlayer)
    {
        PlayerIndex = cPlayer;
    }
}

public class FinishedLevelEvent : Event { }

public class EnemySpawnTriggeredEvent : Event
{
    public int WaveNumber;
    public EnemySpawnTriggeredEvent(int cWaveNumber)
    {
        WaveNumber = cWaveNumber;
    }
}

#endregion

#region Player pickup events

public class PlayerGunPickupEvent : Event
{
    public int Player;
    public int GunIndex;
    public PlayerGunPickupEvent(int cPlayer, int cGunIndex)
    {
        Player = cPlayer;
        GunIndex = cGunIndex;
    }
}

public class PlayerGunSwapEvent : Event
{
    public int Player;
    public int GunIndex;
    public PlayerGunSwapEvent(int cPlayer, int cGunIndex)
    {
        Player = cPlayer;
        GunIndex = cGunIndex;
    }
}

public class PlayerShieldPickupEvent : Event
{
    public int Player;
    public int ShieldAmount;
    public PlayerShieldPickupEvent(int cPlayer, int cShieldAmount)
    {
        Player = cPlayer;
        ShieldAmount = cShieldAmount;
    }
}

public class PlayerShieldDropEvent : Event
{
    public int Player;
    public PlayerShieldDropEvent(int cPlayer)
    {
        Player = cPlayer;
    }
}

#endregion