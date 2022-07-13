using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

interface IGameEventListenerInt2BoardSpace : IGameEventListener
{
    void OnEventRaised(int n1, int n2, BoardSpace bs);
}

[System.Serializable]
public class Int2BoardSpaceEvent : UnityEvent<int, int, BoardSpace> { }

public class GameEventListenerInt2BoardSpace : MonoBehaviour, IGameEventListenerInt2BoardSpace
{
    public GameEvent gameEvent;
    public Int2BoardSpaceEvent response;

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

    public void OnEventRaised(int n1, int n2, BoardSpace bs)
    {
        response.Invoke(n1, n2, bs);
    }
}
