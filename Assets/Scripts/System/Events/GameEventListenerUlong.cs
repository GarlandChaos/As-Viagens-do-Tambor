using UnityEngine;
using UnityEngine.Events;

namespace TamborGame.Events
{
    interface IGameEventListenerUlong : IGameEventListener
    {
        void OnEventRaised(ulong value);
    }

    [System.Serializable]
    public class UlongEvent : UnityEvent<ulong> { }

    public class GameEventListenerUlong : MonoBehaviour, IGameEventListenerUlong
    {
        public GameEvent gameEvent;
        public UlongEvent response;

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

        public void OnEventRaised(ulong value)
        {
            response.Invoke(value);
        }
    }
}
