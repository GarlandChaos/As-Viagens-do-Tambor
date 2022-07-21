using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

interface IGameEventListenerStringArray : IGameEventListener
{
    void OnEventRaised(string[] value);
}

[System.Serializable]
public class StringArrayEvent : UnityEvent<string[]> { }

public class GameEventListenerStringArray : MonoBehaviour, IGameEventListenerStringArray
{
    public GameEvent gameEvent;
    public StringArrayEvent response;

    private void OnEnable()
    {
        gameEvent.RegisterListener(this);
    }

    private void OnDisable()
    {
        gameEvent.UnregisterListener(this);
    }

    private void OnDestroy()
    {
        gameEvent.UnregisterListener(this);
    }

    public void OnEventRaised()
    {
        Debug.Log("Cannot use this version");
    }

    public void OnEventRaised(string[] value)
    {
        response.Invoke(value);
    }
}
