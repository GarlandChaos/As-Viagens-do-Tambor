using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

interface IGameEventListenerQueueBoardSpace : IGameEventListener
{
    void OnEventRaised(Queue<BoardSpace> q);
}

[System.Serializable]
public class QueueBoardSpaceEvent : UnityEvent<Queue<BoardSpace>> { }

public class GameEventListenerQueueBoardSpace : MonoBehaviour, IGameEventListenerQueueBoardSpace
{
    public GameEvent gameEvent;
    public QueueBoardSpaceEvent response;

    private void OnEnable()
    {
        gameEvent.RegisterListener(this);
    }

    private void OnDisable()
    {
        gameEvent.UnregisterListener(this);
    }

    public void OnEventRaised()
    {
        Debug.Log("Cannot use this version");
    }

    public void OnEventRaised(Queue<BoardSpace> q)
    {
        response.Invoke(q);
    }
}
