using UnityEngine;
using UnityEngine.Events;
using TamborGame.Gameplay;

namespace TamborGame.Events
{
    interface IGameEventListenerCardString : IGameEventListener
    {
        void OnEventRaised(Card card, string text);
    }

    [System.Serializable]
    public class CardStringEvent : UnityEvent<Card, string> { }

    public class GameEventListenerCardString : MonoBehaviour, IGameEventListenerCardString
    {
        public GameEvent gameEvent;
        public CardStringEvent response;

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

        public void OnEventRaised(Card card, string text)
        {
            response.Invoke(card, text);
        }
    }
}
