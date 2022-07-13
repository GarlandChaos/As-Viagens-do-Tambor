﻿using System.Collections;
using System.Collections.Generic;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using MLAPI.NetworkVariable.Collections;
using UnityEngine;
using UnityEngine.Events;

//Has an id
//Has a pawn
//Has a bool to check if can roll dices
//Has events
//Has lists of cards of people, practice and place
//Has network variables: lists of cards' ids of people, practice and place and a bool to check turn

//Instantiate Pawn
//Register itself to GameManager
//Request envelope cards from server's GameManager and send them to client's GameManager
//Roll dices when is it's turn
//Add to lists of cards and cards' ids
//Change turns
//Move Pawn

[RequireComponent(typeof(SpriteRenderer))]
public class Player : NetworkBehaviour
{
    //Runtime fields
    [HideInInspector]
    public int id = -1;
    [HideInInspector]
    public bool canRollDices = false;
    List<Card> cardPersonList = new List<Card>(),
        cardPracticeList = new List<Card>(),
        cardPlaceList = new List<Card>(),
        discardedCardList = new List<Card>();
    [HideInInspector]
    public BoardSpace currentBoardSpace;

    //Serialized fields
    [SerializeField]
    GameEvent eventRequestPaths = null,
        eventRequestExtraCard = null,
        eventAskIfWantToGuess = null,
        eventEndOfMove = null,
        eventChangePlayerTurn = null,
        eventStartGame = null,
        eventSendServerPawnToRoomManager = null,
        eventWin = null,
        eventLose = null,
        eventDisplayDiceResults = null,
        eventUpdateAvailablePawns = null;
    [SerializeField]
    SpriteRenderer spriteRenderer = null;
    [SerializeField]
    PawnContainer pawnContainer = null;

    //Properties
    public List<Card> _CardPersonList { get => cardPersonList; }
    public List<Card> _CardPracticeList { get => cardPracticeList; }
    public List<Card> _CardPlaceList { get => cardPlaceList; }

    //Network variables
    NetworkList<int> listPeopleIds = new NetworkList<int>(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });
    NetworkList<int> listPracticesIds = new NetworkList<int>(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });
    NetworkList<int> listPlacesIds = new NetworkList<int>(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });
    NetworkList<int> discardedCardsIds = new NetworkList<int>(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });
    [HideInInspector]
    public NetworkVariableBool isMyTurn = new NetworkVariableBool(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.Everyone,
        ReadPermission = NetworkVariablePermission.Everyone
    }, false);
    [HideInInspector]
    public NetworkVariableVector3 networkPosition = new NetworkVariableVector3(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.Everyone,
        ReadPermission = NetworkVariablePermission.Everyone
    });
    [HideInInspector]
    public NetworkVariableString pawnName = new NetworkVariableString(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.Everyone,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    private void OnDestroy()
    {
        listPeopleIds.OnListChanged -= AddPersonCardToListById;
        listPlacesIds.OnListChanged -= AddPlaceCardToListById;
        listPracticesIds.OnListChanged -= AddPracticeCardToListById;
        discardedCardsIds.OnListChanged -= AddDiscardedCardToListById;
        isMyTurn.OnValueChanged -= OnMyTurnValueChanged;
        pawnName.OnValueChanged -= OnPawnNameValueChanged;
    }

    private void Singleton_OnClientDisconnectCallback(ulong obj) //not working!!!
    {
        NetworkManager.Singleton.OnClientDisconnectCallback -= Singleton_OnClientDisconnectCallback;
        Debug.Log("Disconnected prefab from client: " + obj);

        //Destroy(gameObject);
    }

    private void Start()
    {
        listPeopleIds.OnListChanged += AddPersonCardToListById;
        listPlacesIds.OnListChanged += AddPlaceCardToListById;
        listPracticesIds.OnListChanged += AddPracticeCardToListById;
        discardedCardsIds.OnListChanged += AddDiscardedCardToListById;
        isMyTurn.OnValueChanged += OnMyTurnValueChanged;
        pawnName.OnValueChanged += OnPawnNameValueChanged;
        NetworkManager.Singleton.OnClientDisconnectCallback += Singleton_OnClientDisconnectCallback;

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
        if (GameManager.instance != null)
        {
            if (!GameManager.instance._RegisterPlayerLock)
                GameManager.instance.RegisterPlayer(this);
            else
                StartCoroutine(WaitToRegisterCoroutine()); //RegisterLock is on, starting coroutine.
        }
        else
            StartCoroutine(WaitToRegisterCoroutine()); //GameManager is null, starting coroutine.
        
        if (!IsServer && IsOwner)
            RequestEnvelopeCardsServerRpc();

        networkPosition.Value = transform.position;
    }

    public void AddToCardLists(Card card)
    {
        if(card.type == CardType.person)
        {
            cardPersonList.Add(card);
            listPeopleIds.Add(card.id);
        }
        else if(card.type == CardType.practice)
        {
            cardPracticeList.Add(card);
            listPracticesIds.Add(card.id);
        }
        else if(card.type == CardType.place)
        {
            cardPlaceList.Add(card);
            listPlacesIds.Add(card.id);
        }
    }

    public void AddToDiscardedCards(Card card)
    {
        if (!discardedCardList.Contains(card))
        {
            discardedCardList.Add(card);
            discardedCardsIds.Add(card.id);
        }
    }

    public bool IsItADiscardedCard(Card card)
    {
        return discardedCardList.Contains(card);
    }

    public void RollDices()
    {
        Dices.RollDices();
        canRollDices = false;
        eventDisplayDiceResults.Raise();
    }

    public void OnActUponDiceResults()
    {
        if (isMyTurn.Value && IsOwner)
        {
            if (Dices._Dice1Result != Dices._Dice2Result)
                eventRequestPaths.Raise(Dices._Dice1Result, Dices._Dice2Result, currentBoardSpace);
            else
                eventRequestExtraCard.Raise();
        }
    }

    public void OnFinalBoardSpacePressed(Queue<BoardSpace> path)
    {
        MovePawn(path, true);
    }

    public void MovePawn(Queue<BoardSpace> path, bool askIfWantToGuess)
    {
        if (isMyTurn.Value && IsOwner)
        {
            if (IsServer)
                MovePawnClientRpc(Board.instance.GetPathIds(path)); //send client rpc to move
            else
                MovePawnServerRpc(Board.instance.GetPathIds(path)); //send server rpc to move
            
            StartCoroutine(MoveCoroutine(path, askIfWantToGuess));
        }
    }

    public void SetPlayerPawn(ulong clientID, string pawnName)
    {
        if (OwnerClientId == clientID)
            this.pawnName.Value = pawnName;
    }

    public void OnRequestPawns()
    {
        if (!IsServer && IsOwner)
            RequestPawnsServerRpc();
    }

    [ServerRpc]
    public void SendWinEventServerRpc()
    {
        eventWin.Raise();
    }

    [ClientRpc]
    public void SendWinEventClientRpc()
    {
        if (!IsServer)
            eventWin.Raise();
    }

    [ServerRpc]
    public void SendLoseEventServerRpc()
    {
        eventLose.Raise();
    }

    [ClientRpc]
    public void SendLoseEventClientRpc()
    {
        if (!IsServer)
            eventLose.Raise();
    }

    [ServerRpc]
    void MovePawnServerRpc(int[] ids)
    {
        StartCoroutine(MoveCoroutine(Board.instance.GetBoardSpacesByIds(ids), false));
    }

    [ClientRpc]
    void MovePawnClientRpc(int[] ids)
    {
        if (!IsServer)
            StartCoroutine(MoveCoroutine(Board.instance.GetBoardSpacesByIds(ids), false));
    }

    [ServerRpc]
    void RequestPawnsServerRpc()
    {
        CreatePawnCardsClientRpc(GetPlayersPawnNames());       
    }

    [ClientRpc]
    void CreatePawnCardsClientRpc(string[] pawnNames)
    {
        if (IsServer)
            return;

        eventSendServerPawnToRoomManager.Raise(pawnNames);
    }

    string[] GetPlayersPawnNames()
    {
        string[] pawnNames = new string[NetworkManager.Singleton.ConnectedClientsList.Count];
        List<MLAPI.Connection.NetworkClient> clientList = NetworkManager.Singleton.ConnectedClientsList;
        
        for (int i = 0; i < clientList.Count; i++)
            pawnNames[i] = clientList[i].PlayerObject.GetComponent<Player>().pawnName.Value;

        return pawnNames;
    }

    Pawn InstantiatePawn(Pawn pawn, Vector3 positionOffset, Transform parent)
    {
        Pawn p = Instantiate(pawn);
        p.GetComponent<NetworkObject>().Spawn();
        p.transform.position += positionOffset;
        p.transform.SetParent(parent, false);
        
        return p;
    }

    [ServerRpc]
    void RequestEnvelopeCardsServerRpc()
    {
        SendEvelopeCardsClientRpc(GameManager.instance._EnvelopePerson, GameManager.instance._EnvelopePractice, GameManager.instance._EnvelopePlace);
    }

    [ClientRpc]
    void SendEvelopeCardsClientRpc(int personID, int practiceID, int placeID)
    {
        if (IsServer) return;
        GameManager.instance.FillEnvelope(personID, practiceID, placeID);
    }

    void AddPersonCardToListById(NetworkListEvent<int> changeEvent)
    {
        Card c = GameManager.instance.GetCardByTypeAndID(CardType.person, changeEvent.Value);
        if (!cardPersonList.Contains(c))
            cardPersonList.Add(c);
    }

    void AddPlaceCardToListById(NetworkListEvent<int> changeEvent)
    {
        Card c = GameManager.instance.GetCardByTypeAndID(CardType.place, changeEvent.Value);
        if (!cardPlaceList.Contains(c))
            cardPlaceList.Add(c);
    }

    void AddPracticeCardToListById(NetworkListEvent<int> changeEvent)
    {
        Card c = GameManager.instance.GetCardByTypeAndID(CardType.practice, changeEvent.Value);
        if (!cardPracticeList.Contains(c))
            cardPracticeList.Add(c);
    }

    void AddDiscardedCardToListById(NetworkListEvent<int> changeEvent)
    {
        Card c = GameManager.instance.GetCardByID(changeEvent.Value);
        if (!discardedCardList.Contains(c))
            discardedCardList.Add(c);
    }

    void OnMyTurnValueChanged(bool previous, bool current)
    {
        if (!previous)
        {
            if (IsOwner && !canRollDices)
                canRollDices = true;

            if (isMyTurn.Value && IsOwner && canRollDices)
                RollDices();
        }
    }

    void OnPawnNameValueChanged(string previous, string current)
    {
        if(!string.IsNullOrEmpty(current))
            spriteRenderer.sprite = pawnContainer.GetPawnByName(pawnName.Value).spritePawn;

        if (IsOwner)
            eventUpdateAvailablePawns.Raise(GetPlayersPawnNames());
    }

    public void RequestPlayerToSendDiscardCardEvents()
    {
        SendDiscardCardEventsClientRpc();
    }

    [ClientRpc]
    void SendDiscardCardEventsClientRpc()
    {
        if (IsServer) 
            return;

        //cuidar que os cards do envelope estão diferentes para cada jogador.
        //o server retira o envelope e comunica os clientes.
        //comunicar o GameManager para disparar os eventos de descarte no bloco de notas
        GameManager.instance.SendDiscardCardEvents(this);
    }

    public void SendStartGameRPC()
    {
        if (IsServer && IsOwner)
            if (NetworkManager.Singleton.ConnectedClientsList.Count < 5)
                StartGameClientRpc();
    }

    public void DisconnectPlayer()
    {
        if (OwnerClientId == NetworkManager.Singleton.LocalClientId)
        {
            //MyNetworkDiscovery.instance.StopBroadcast();
            //NetworkManager.Singleton.StopClient(); 
            DisconnectPlayerServerRPC(NetworkManager.Singleton.LocalClientId);
            //NetworkManager.Singleton.NetworkConfig.NetworkTransport.Shutdown();
            //NetworkManager.Singleton.Shutdown();
        }
    }

    [ServerRpc]
    public void DisconnectPlayerServerRPC(ulong clientID)
    {
        //if (IsServer && IsOwner)
            NetworkManager.Singleton.DisconnectClient(clientID); Debug.Log("Disconnected client: " + clientID);
    }

    [ClientRpc]
    void StartGameClientRpc()
    {
        eventStartGame.Raise();
    }

    [ServerRpc]
    void ChangePlayerTurnServerRpc()
    {
        ChangePlayerTurnClientRpc();
    }

    [ClientRpc]
    void ChangePlayerTurnClientRpc()
    {
        eventChangePlayerTurn.Raise();
    }

    public void ChangePlayerTurn()
    {
        if (IsServer && IsOwner)
            ChangePlayerTurnClientRpc();
        else if(!IsServer && IsOwner)
            ChangePlayerTurnServerRpc();
    }

    public void ActivateReroll()
    {
        canRollDices = true;
    }

    public void MoveToPlaceFromExtraCard(ExtraCard extraCard)
    {
        if (isMyTurn.Value && IsOwner)
        {
            BoardSpace goal = Board.instance.GetPlaceByEnum(extraCard.placeToGo);
            MovePawn(AStar.CalculatePathToPlace(currentBoardSpace, goal), extraCard.askIfWantToGuess);
        }
    }

    IEnumerator WaitToRegisterCoroutine()
    {
        yield return new WaitForSeconds(1f);

        if(GameManager.instance != null)
        {
            if (!GameManager.instance._RegisterPlayerLock)
                GameManager.instance.RegisterPlayer(this);
            else
                StartCoroutine(WaitToRegisterCoroutine());
        }
        else
            StartCoroutine(WaitToRegisterCoroutine());
    }

    IEnumerator MoveCoroutine(Queue<BoardSpace> path, bool askIfWantToGuess)
    {
        WaitForEndOfFrame wait = new WaitForEndOfFrame();
        AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        float timer = 0f;
        Vector3 startPos = transform.position;
        BoardSpace bs = path.Dequeue();
        Vector3 endPos = bs.transform.position;

        while (timer < 1f)
        {
            timer += Time.deltaTime * 2f;
            transform.position = Vector3.Lerp(startPos, endPos, curve.Evaluate(timer));
            
            yield return wait;
        }

        if (path.Count > 0)
            StartCoroutine(MoveCoroutine(path, askIfWantToGuess));
        else
        {
            currentBoardSpace = bs;
            if (IsOwner)
            {
                eventEndOfMove.Raise();
                Place place = bs.GetComponent<Place>();
                if (place != null)
                {
                    if (askIfWantToGuess)
                        eventAskIfWantToGuess.Raise(place.cardPlace);
                    else
                        ChangePlayerTurn();
                }
                else
                    ChangePlayerTurn();

                eventDisplayDiceResults.Raise();
            }
        }
    }
}
