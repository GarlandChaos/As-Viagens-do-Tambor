﻿using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class GameEvent : ScriptableObject
{
    private List<IGameEventListener> listeners = new List<IGameEventListener>();

    public void Raise(params object[] args)
    {
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            //DELETE IF NOT USED UNTIL RELEASE
            IGameEventListenerInt2BoardSpace listenerInt2BS = listeners[i] as IGameEventListenerInt2BoardSpace;
            if (listenerInt2BS != null)
            {
                listenerInt2BS.OnEventRaised((int)args[0], (int)args[1], (BoardSpace)args[2]);
                continue;
            }

            IGameEventListenerQueueBoardSpace listenerQueueBS = listeners[i] as IGameEventListenerQueueBoardSpace;
            if (listenerQueueBS != null)
            {
                listenerQueueBS.OnEventRaised((Queue<BoardSpace>)args[0]);
                continue;
            }

            IGameEventListenerInt2 listenerInt2 = listeners[i] as IGameEventListenerInt2;
            if (listenerInt2 != null)
            {
                listenerInt2.OnEventRaised((int)args[0], (int)args[1]);
                continue;
            }

            IGameEventListenerString listenerString = listeners[i] as IGameEventListenerString;
            if (listenerString != null)
            {
                listenerString.OnEventRaised((string)args[0]);
                continue;
            }

            IGameEventListenerBool listenerBool = listeners[i] as IGameEventListenerBool;
            if (listenerBool != null)
            {
                listenerBool.OnEventRaised((bool)args[0]);
                continue;
            }

            IGameEventListenerCard listenerCard = listeners[i] as IGameEventListenerCard;
            if (listenerCard != null)
            {
                listenerCard.OnEventRaised((Card)args[0]);
                continue;
            }

            IGameEventListenerExtraCard listenerExtraCard = listeners[i] as IGameEventListenerExtraCard;
            if (listenerExtraCard != null)
            {
                listenerExtraCard.OnEventRaised((ExtraCard)args[0]);
                continue;
            }

            IGameEventListenerCardInt listenerCardInt = listeners[i] as IGameEventListenerCardInt;
            if (listenerCardInt != null)
            {
                listenerCardInt.OnEventRaised((Card)args[0], (int)args[1]);
                continue;
            }

            //IGameEventListenerFloat listenerFloat = listeners[i] as IGameEventListenerFloat;
            //if (listenerFloat != null)
            //{
            //    listenerFloat.OnEventRaised((float)args[0]);
            //    continue;
            //}
            listeners[i].OnEventRaised();
        }
    }

    public void RegisterListener(IGameEventListener listener)
    {
        listeners.Add(listener);
    }

    public void UnregisterListener(IGameEventListener listener)
    {
        listeners.Remove(listener);
    }
}