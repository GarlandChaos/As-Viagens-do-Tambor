using UnityEngine;
using UnityEngine.Events;

public interface IGameEventListener
{
    void OnEventRaised();
}

//DELETE IF NOT USED UNTIL RELEASE
//public interface IGameEventListenerString : IGameEventListener
//{
//    void OnEventRaised(string element);
//}

//public interface IGameEventListenerFloat : IGameEventListener
//{
//    void OnEventRaised(float value);
//}

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

    public void OnEventRaised()
    {
        response.Invoke();
    }
}