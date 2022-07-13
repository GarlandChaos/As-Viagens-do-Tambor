using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

interface IGameEventListenerString : IGameEventListener
{
    void OnEventRaised(string text);
}

[System.Serializable]
public class StringEvent : UnityEvent<string> { }

public class GameEventListenerString : MonoBehaviour, IGameEventListenerString
{
    public GameEvent gameEvent;
    public StringEvent response;

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

    public void OnEventRaised(string text)
    {
        response.Invoke(text);
    }
}
