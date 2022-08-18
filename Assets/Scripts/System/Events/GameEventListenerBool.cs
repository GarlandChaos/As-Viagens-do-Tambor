using UnityEngine;
using UnityEngine.Events;

namespace TamborGame.Events
{
    interface IGameEventListenerBool : IGameEventListener
    {
        void OnEventRaised(bool value);
    }

    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    public class GameEventListenerBool : MonoBehaviour, IGameEventListenerBool
    {
        public GameEvent gameEvent;
        public BoolEvent response;

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

        public void OnEventRaised(bool value)
        {
            response.Invoke(value);
        }
    }
}
