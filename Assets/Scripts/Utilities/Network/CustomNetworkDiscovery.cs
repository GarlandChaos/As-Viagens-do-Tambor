using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TamborGame.Events;

namespace TamborGame.Utilities
{
    [RequireComponent(typeof(GameEventEmitter))]
    public class CustomNetworkDiscovery : NetworkDiscovery
    {
        public static CustomNetworkDiscovery instance = null;
        public Dictionary<string, string> addresses = new Dictionary<string, string>();
        GameEventEmitter emitterUpdateRooms = null;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                emitterUpdateRooms = GetComponent<GameEventEmitter>();
            }
            else
                Destroy(gameObject);
        }

        public override void OnReceivedBroadcast(string fromAddress, string data)
        {
            if (!addresses.ContainsKey(fromAddress))
            {
                addresses.Add(fromAddress, data);
                emitterUpdateRooms.EmitEvent();
            }
        }
    }
}