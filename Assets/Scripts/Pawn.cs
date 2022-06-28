using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
//using MLAPI.Serialization;

[RequireComponent(typeof(SpriteRenderer))]
public class Pawn : NetworkBehaviour//, INetworkSerializable
{
    public string namePawn;
    public BoardSpace currentBoardSpace;
    public Sprite spritePawn;
    public NetworkVariableVector3 networkPosition = new NetworkVariableVector3(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.Everyone,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    //private void Awake()
    //{
    //    if(spritePawn == null)
    //    {
    //        spritePawn = GetComponent<SpriteRenderer>().sprite;
    //    }
    //}

    private void Start()
    {
        //COMEÇO GAMBIARRA
        Board.instance.gameObject.SetActive(true);
        //FIM GAMBIARRA

        currentBoardSpace = Board.instance.initialSpace;

        //COMEÇO GAMBIARRA
        Board.instance.gameObject.SetActive(false);
        //FIM GAMBIARRA
    }

    public override void NetworkStart()
    {
        networkPosition.Value = transform.position;
    }

    private void Update()
    {
        //UpdatePosition();
    }

    //public void UpdatePosition()
    //{
    //    transform.position = networkPosition.Value;
    //}

    [ClientRpc]
    public void SetClientParentClientRpc(ulong clientID)
    {
        if (IsServer) return;
        transform.SetParent(NetworkManager.ConnectedClients[clientID].PlayerObject.transform, false);
        transform.parent.GetComponent<Player>()._PlayerPawn = this;
    }

    //// INetworkSerializable
    //public void NetworkSerialize(NetworkSerializer serializer)
    //{
    //    serializer.Serialize(ref namePawn);
    //    serializer.Serialize(ref currentBoardSpace);
    //    serializer.Serialize(ref spritePawn);
    //}
    //// ~INetworkSerializable
}
