using UnityEngine;
using UnityEngine.Events;

namespace TamborGame.Events
{
    interface IGameEventListenerUlongString : IGameEventListener
    {
        void OnEventRaised(ulong value, string text);
    }

    [System.Serializable]
    public class UlongStringEvent : UnityEvent<ulong, string> { }

    public class GameEventListenerUlongString : MonoBehaviour, IGameEventListenerUlongString
    {
        public GameEvent gameEvent;
        public UlongStringEvent response;

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

        public void OnEventRaised(ulong value, string text)
        {
            response.Invoke(value, text);
        }
    }
}