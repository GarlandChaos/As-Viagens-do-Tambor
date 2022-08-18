using UnityEngine;
using UnityEngine.Events;

namespace TamborGame.Events
{
    interface IGameEventListenerInt3 : IGameEventListener
    {
        void OnEventRaised(int value1, int value2, int value3);
    }

    [System.Serializable]
    public class Int3Event : UnityEvent<int, int, int> { }

    public class GameEventListenerInt3 : MonoBehaviour, IGameEventListenerInt3
    {
        public GameEvent gameEvent;
        public Int3Event response;

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

        public void OnEventRaised(int value1, int value2, int value3)
        {
            response.Invoke(value1, value2, value3);
        }
    }
}
