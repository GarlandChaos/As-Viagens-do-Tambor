using UnityEngine;

namespace TamborGame.Events
{
    public class GameEventEmitter : MonoBehaviour
    {
        public GameEvent Event;

        public void EmitEvent()
        {
            Event.Raise();
        }
    }
}
