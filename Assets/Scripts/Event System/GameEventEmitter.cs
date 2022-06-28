using UnityEngine;

public class GameEventEmitter : MonoBehaviour
{
    public GameEvent Event;

    public void EmitEvent()
    {    
        Event.Raise();
    }
}
