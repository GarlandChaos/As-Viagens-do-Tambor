using UnityEngine;
using UnityEngine.Events;

namespace TamborGame.Events
{
    public interface IGameEventListener
    {
        void OnEventRaised();
    }

    public class GameEventListener : MonoBehaviour, IGameEventListener
    {
        public GameEvent gameEvent;
        public UnityEvent response;

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
            response.Invoke();
        }
    }
}