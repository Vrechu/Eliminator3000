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

#region Playerevents

public class PlayerPickedupItemEvent : Event
{
    public GameObject Player;
    public GameObject Item;
    public PlayerPickedupItemEvent(GameObject cPlayer, GameObject cItem)
    {
        Player = cPlayer;
        Item = cItem;
    }
}
#endregion