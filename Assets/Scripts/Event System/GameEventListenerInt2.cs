using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

interface IGameEventListenerInt2 : IGameEventListener
{
    void OnEventRaised(int n1, int n2);
}

[System.Serializable]
public class Int2Event : UnityEvent<int, int> { }

public class GameEventListenerInt2 : MonoBehaviour, IGameEventListenerInt2
{
    public GameEvent gameEvent;
    public Int2Event response;

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

    public void OnEventRaised(int n1, int n2)
    {
        response.Invoke(n1, n2);
    }
}
