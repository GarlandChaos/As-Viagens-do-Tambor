﻿using UnityEngine;
using UnityEngine.Events;
using TamborGame.Gameplay;

namespace TamborGame.Events
{
    interface IGameEventListenerCardInt : IGameEventListener
    {
        void OnEventRaised(Card card, int n);
    }

    [System.Serializable]
    public class CardIntEvent : UnityEvent<Card, int> { }

    public class GameEventListenerCardInt : MonoBehaviour, IGameEventListenerCardInt
    {
        public GameEvent gameEvent;
        public CardIntEvent response;

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

        public void OnEventRaised(Card card, int n)
        {
            response.Invoke(card, n);
        }
    }
}
