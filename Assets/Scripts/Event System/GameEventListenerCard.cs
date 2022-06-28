﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

interface IGameEventListenerCard : IGameEventListener
{
    void OnEventRaised(Card card);
}

[System.Serializable]
public class CardEvent : UnityEvent<Card> { }

public class GameEventListenerCard : MonoBehaviour, IGameEventListenerCard
{
    public GameEvent gameEvent;
    public CardEvent response;

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

    public void OnEventRaised(Card card)
    {
        response.Invoke(card);
    }
}
