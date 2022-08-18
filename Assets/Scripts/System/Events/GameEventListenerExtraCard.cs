using UnityEngine;
using UnityEngine.Events;
using TamborGame.Gameplay;

namespace TamborGame.Events
{
    interface IGameEventListenerExtraCard : IGameEventListener
    {
        void OnEventRaised(ExtraCard extraCard);
    }

    [System.Serializable]
    public class ExtraCardEvent : UnityEvent<ExtraCard> { }

    public class GameEventListenerExtraCard : MonoBehaviour, IGameEventListenerExtraCard
    {
        public GameEvent gameEvent;
        public ExtraCardEvent response;

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

        public void OnEventRaised(ExtraCard extraCard)
        {
            response.Invoke(extraCard);
        }
    }
}
