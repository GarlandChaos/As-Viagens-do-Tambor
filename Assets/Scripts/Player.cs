using System.Collections;
using System.Collections.Generic;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using MLAPI.NetworkVariable.Collections;
using UnityEngine;

//Add to lists of cards
//Instantiate Pawn
//Move Pawn
public class Player : NetworkBehaviour
{
    //Runtime fields
    public int id = -1;
    public bool canRollDices = false;

    //Serialized fields
    [SerializeField]
    Pawn playerPawn;
    [SerializeField]
    GameEvent eventRequestPaths, 
        eventRequestExtraCard, 
        eventAskIfWantToGuess, 
        eventEndOfMove, 
        eventChangePlayerTurn, 
        eventStartGame, 
        eventSendServerPawnToRoomManager,
        eventWin,
        eventLose;
    [SerializeField]
    List<Card> listPeople = new List<Card>(), listPractices = new List<Card>(), listPlaces = new List<Card>(), discardedCards = new List<Card>();

    //Properties
    public Pawn _PlayerPawn { set { playerPawn = value; } }

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
    public NetworkVariableBool isMyTurn = new NetworkVariableBool(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.Everyone,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    private void Awake()
    {
        isMyTurn.Value = false;
        listPeopleIds.OnListChanged += AddPersonCardToListById;
        listPlacesIds.OnListChanged += AddPlaceCardToListById;
        listPracticesIds.OnListChanged += AddPracticeCardToListById;
        discardedCardsIds.OnListChanged += AddDiscardedCardToListById;
    }

    private void OnDestroy()
    {
        listPeopleIds.OnListChanged -= AddPersonCardToListById;
        listPlacesIds.OnListChanged -= AddPlaceCardToListById;
        listPracticesIds.OnListChanged -= AddPracticeCardToListById;
        discardedCardsIds.OnListChanged -= AddDiscardedCardToListById;
    }

    private void Start()
    {
        if (!IsServer && playerPawn == null && transform.childCount > 0)
        {
            playerPawn = GetComponentInChildren<Pawn>();
            Debug.Log("Setou player pawn para " + playerPawn.namePawn);
        }
    }

    public override void NetworkStart() //conferir se ocorre antes ou depois de Start()
    {
        Debug.Log("NetworkManager.Singleton.LocalClientId: " + NetworkManager.Singleton.LocalClientId);

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
    }

    void Update()
    {
        if (isMyTurn.Value && IsOwner && canRollDices)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                RollDices();
                canRollDices = false;
            }
        }
    }

    public void AddToCardLists(Card card)
    {
        if(card.type == CardType.person)
        {
            listPeople.Add(card);
            listPeopleIds.Add(card.id);
        }
        else if(card.type == CardType.practice)
        {
            listPractices.Add(card);
            listPracticesIds.Add(card.id);
        }
        else if(card.type == CardType.place)
        {
            listPlaces.Add(card);
            listPlacesIds.Add(card.id);
        }
    }

    public void AddToDiscardedCards(Card card)
    {
        if (!discardedCards.Contains(card))
        {
            discardedCards.Add(card);
            discardedCardsIds.Add(card.id);
        }
    }

    public bool IsItADiscardedCard(Card card)
    {
        return discardedCards.Contains(card);
    }

    public List<Card> GetPersonCards() //could be a property
    {
        return listPeople;
    }

    public List<Card> GetPracticeCards() //could be a property
    {
        return listPractices;
    }

    public List<Card> GetPlaceCards() //could be a property
    {
        return listPlaces;
    }

    public void RollDices()
    {
        int[] dices = new int[2];
        dices = Dices.instance.RollDices();
        if (dices[0] != dices[1])
            eventRequestPaths.Raise(dices[0], dices[1], playerPawn.currentBoardSpace);
        else
            eventRequestExtraCard.Raise();
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

    void CreatePlayerPawn()
    {
        if (IsOwner)
        {
            if (IsServer)
            {
                Pawn p = Instantiate(GameManager.instance.clientPawn);
                p.GetComponent<NetworkObject>().Spawn();
                p.transform.position -= new Vector3(0.5f, 0f, 0f);
                p.transform.SetParent(NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.transform, false);
                playerPawn = p;
            }
            else
                SpawnClientPawnServerRpc(NetworkManager.Singleton.LocalClientId, GameManager.instance.clientPawn.name); //precisa ser feito no servidor o primeiro instanciamento
        }
        
    }

    public void OnRequestPawns()
    {
        if (!IsServer && IsOwner)
            RequestPawnsServerRpc(NetworkManager.Singleton.LocalClientId);
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
    void RequestPawnsServerRpc(ulong clientID)
    {
#if UNITY_EDITOR
        Debug.Log("Is owned by server: " + IsOwnedByServer);
        Debug.Log("Is server: " + IsServer);
        Debug.Log("Is owner: " + IsOwner);
        Debug.Log("Requisitando pawns, Server's ID is " + NetworkManager.Singleton.LocalClientId + ", client's ID is " + clientID);
#endif
        CreatePawnCardsClientRpc(NetworkManager.Singleton.LocalClientId, GameManager.instance.clientPawn.name);       
    }

    [ClientRpc]
    void CreatePawnCardsClientRpc(ulong serverID, string serverPawn)
    {
#if UNITY_EDITOR
        Debug.Log("Is owned by server: " + IsOwnedByServer);
        Debug.Log("Is server: " + IsServer);
        Debug.Log("Is owner: " + IsOwner);
#endif
        if (IsServer) { return; }
#if UNITY_EDITOR
        Debug.Log("Creating pawns without the host pawn " + serverPawn + ". This clientID is " + NetworkManager.Singleton.LocalClientId);
#endif
        eventSendServerPawnToRoomManager.Raise(serverPawn);
    }

    [ServerRpc]
    void SpawnClientPawnServerRpc(ulong clientID, string clientPawn)
    {
        Pawn p = Instantiate(GameManager.instance.GetPawnByName(clientPawn));
        p.transform.position += new Vector3(1f, 0f, 0f);
        p.GetComponent<NetworkObject>().Spawn();
        p.transform.SetParent(transform, false);
        playerPawn = p;
        playerPawn.SetClientParentClientRpc(clientID);
        //SetClientPlayerPawnClientRpc(clientID);
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
        if (!listPeople.Contains(c))
            listPeople.Add(c);
    }

    void AddPlaceCardToListById(NetworkListEvent<int> changeEvent)
    {
        Card c = GameManager.instance.GetCardByTypeAndID(CardType.place, changeEvent.Value);
        if (!listPlaces.Contains(c))
            listPlaces.Add(c);
    }

    void AddPracticeCardToListById(NetworkListEvent<int> changeEvent)
    {
        Card c = GameManager.instance.GetCardByTypeAndID(CardType.practice, changeEvent.Value);
        if (!listPractices.Contains(c))
            listPractices.Add(c);
    }

    void AddDiscardedCardToListById(NetworkListEvent<int> changeEvent)
    {
        Card c = GameManager.instance.GetCardByID(changeEvent.Value);
        if (!discardedCards.Contains(c))
            discardedCards.Add(c);
    }

    public void RequestPlayerToSendDiscardCardEvents()
    {
        SendDiscardCardEventsClientRpc();
    }

    [ClientRpc]
    void SendDiscardCardEventsClientRpc()
    {
        if (IsServer) return;

        //cuidar que os cards do envelope estão diferentes para cada jogador.
        //o server retira o envelope e comunica os clientes.
        //comunicar o GameManager para disparar os eventos de descarte no bloco de notas
        GameManager.instance.SendDiscardCardEvents(this);
    }

    public void SendStartGameRPC()
    {
        if (!IsServer && IsOwner) //apenas quando não for servidor, ou seja, quando é garantido que tenha dois jogadores
            StartGameWithTwoPlayersServerRpc();
    }

    [ServerRpc]
    void StartGameWithTwoPlayersServerRpc()
    {
        Debug.Log("Server RPC sent to start game.");
        StartGameWithTwoPlayersClientRpc();
    }

    [ClientRpc]
    void StartGameWithTwoPlayersClientRpc()
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
            MovePawn(AStar.CalculatePathToPlace(playerPawn.currentBoardSpace, goal), extraCard.askIfWantToGuess);
        }
    }

    IEnumerator WaitToRegisterCoroutine()
    {
        yield return new WaitForSeconds(1f);

        if(GameManager.instance != null)
        {
            if (!GameManager.instance._RegisterPlayerLock)
            {
                GameManager.instance.RegisterPlayer(this);
                if (!IsServer && IsOwner)
                    StartGameWithTwoPlayersServerRpc();
            }
            else
                StartCoroutine(WaitToRegisterCoroutine());
        }
        else
            StartCoroutine(WaitToRegisterCoroutine());
    }

    IEnumerator MoveCoroutine(Queue<BoardSpace> path, bool askIfWantToGuess)
    {
        AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        float timer = 0f;
        Vector3 startPos = playerPawn.transform.position;
        BoardSpace bs = path.Dequeue();
        Vector3 endPos = bs.transform.position;
        
        while (timer < 1f)
        {
            timer += Time.deltaTime * 2f;
            playerPawn.transform.position = Vector3.Lerp(startPos, endPos, curve.Evaluate(timer));
            yield return new WaitForEndOfFrame();
        }

        if(path.Count > 0)
            StartCoroutine(MoveCoroutine(path, askIfWantToGuess));
        else
        {
            playerPawn.currentBoardSpace = bs;
            if (IsOwner)
            {
                eventEndOfMove.Raise();
                if (bs.GetComponent<Place>())
                {
                    if (askIfWantToGuess)
                        eventAskIfWantToGuess.Raise(bs.gameObject.GetComponent<Place>().cardPlace);
                    else
                        ChangePlayerTurn();
                }
                else
                    ChangePlayerTurn();
            }
        }
    }
}
