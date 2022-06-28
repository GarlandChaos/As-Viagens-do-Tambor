using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Transports.UNET;
using UnityEngine.Networking;

[RequireComponent(typeof(GameEventEmitter))]
public class MyNetworkDiscovery : NetworkDiscovery
{
    public static MyNetworkDiscovery instance;
    public Dictionary<string, string> addresses;
    GameEventEmitter emitterUpdateRooms;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            addresses = new Dictionary<string, string>();
            emitterUpdateRooms = GetComponent<GameEventEmitter>();
        }
        else
            Destroy(gameObject);
    }

    public override void OnReceivedBroadcast(string fromAddress, string data)
    {
        Debug.Log("Received broadcast from: " + fromAddress + " with the data: " + data);
        if (!addresses.ContainsKey(fromAddress))
        {
            addresses.Add(fromAddress, data);
            //addresses.Add("143.54.201.27", "teste");
            //addresses.Add("143.54.201.28", "teste2");
            emitterUpdateRooms.EmitEvent();
        }
        //MLAPI.NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = fromAddress;
    }
}
