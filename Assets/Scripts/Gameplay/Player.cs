using System.Collections;
using System.Collections.Generic;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using MLAPI.NetworkVariable.Collections;
using UnityEngine;

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
    //Pawn playerPawn;
    public BoardSpace currentBoardSpace;

    //Serialized fields
    [SerializeField]
    GameEvent eventRequestPaths, 
        eventRequestExtraCard,
        eventAskIfWantToGuess, 
        eventEndOfMove, 
        eventChangePlayerTurn, 
        eventStartGame, 
        eventSendServerPawnToRoomManager,
        eventWin,
        eventLose,
        eventDisplayDiceResults;
    [SerializeField]
    SpriteRenderer spriteRenderer = null;
    [SerializeField]
    PawnContainer pawnContainer = null;

    //Properties
    //public Pawn _PlayerPawn { set { playerPawn = value; } }
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

    public NetworkVariableVector3 networkPosition = new NetworkVariableVector3(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.Everyone,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    public NetworkVariableString pawnName = new NetworkVariableString(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.Everyone,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    private void Awake()
    {
        listPeopleIds.OnListChanged += AddPersonCardToListById;
        listPlacesIds.OnListChanged += AddPlaceCardToListById;
        listPracticesIds.OnListChanged += AddPracticeCardToListById;
        discardedCardsIds.OnListChanged += AddDiscardedCardToListById;
        isMyTurn.OnValueChanged += OnMyTurnValueChanged;
        pawnName.OnValueChanged += OnPawnNameValueChanged;
    }

    private void OnDestroy()
    {
        listPeopleIds.OnListChanged -= AddPersonCardToListById;
        listPlacesIds.OnListChanged -= AddPlaceCardToListById;
        listPracticesIds.OnListChanged -= AddPracticeCardToListById;
        discardedCardsIds.OnListChanged -= AddDiscardedCardToListById;
        isMyTurn.OnValueChanged -= OnMyTurnValueChanged;
        pawnName.OnValueChanged -= OnPawnNameValueChanged;
    }

    private void Start()
    {
        //if (!IsServer && playerPawn == null && transform.childCount > 0)
        //    playerPawn = GetComponentInChildren<Pawn>();

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
        //CreatePlayerPawnServerRPC(clientID, pawnName);
        if (OwnerClientId == clientID)
        {
            //if (IsServer)
            //    playerPawn = InstantiatePawn(GameManager.instance.clientPawn, new Vector3(-0.5f, 0f, 0f), NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.transform);
            //else
            //    SpawnClientPawnServerRpc(NetworkManager.Singleton.LocalClientId, GameManager.instance.clientPawn.name); //precisa ser feito no servidor o primeiro instanciamento

            Debug.Log("Calling on clientID " + clientID + " with pawnName: " + pawnName);
            this.pawnName.Value = pawnName;
            //Debug.Log("Should've set to " + GameManager.instance.clientPawn.spritePawn);
        }
    }

    //[ServerRpc]
    //void CreatePlayerPawnServerRPC(ulong clientID, string pawnName)
    //{
    //    CreatePlayerPawnClientRPC(clientID, pawnName);
    //}

    //[ClientRpc]
    //void CreatePlayerPawnClientRPC(ulong clientID, string pawnName)
    //{
    //    Debug.Log("Called CreatePlayerPawn");
    //    if (OwnerClientId == clientID)
    //    {
    //        //if (IsServer)
    //        //    playerPawn = InstantiatePawn(GameManager.instance.clientPawn, new Vector3(-0.5f, 0f, 0f), NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.transform);
    //        //else
    //        //    SpawnClientPawnServerRpc(NetworkManager.Singleton.LocalClientId, GameManager.instance.clientPawn.name); //precisa ser feito no servidor o primeiro instanciamento

    //        this.pawnName.Value = pawnName;
    //        //Debug.Log("Should've set to " + GameManager.instance.clientPawn.spritePawn);
    //    }
    //}

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
        CreatePawnCardsClientRpc(NetworkManager.Singleton.LocalClientId, GameManager.instance.clientPawn.name);       
    }

    [ClientRpc]
    void CreatePawnCardsClientRpc(ulong serverID, string serverPawn)
    {
        if (IsServer)
            return;

        eventSendServerPawnToRoomManager.Raise(serverPawn);
    }

    [ServerRpc]
    void SpawnClientPawnServerRpc(ulong clientID, string clientPawn)
    {
        //playerPawn = InstantiatePawn(GameManager.instance.GetPawnByName(clientPawn), new Vector3(1f, 0f, 0f), transform);
        //playerPawn.SetClientParentClientRpc(clientID);
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
        Debug.Log("previous: " + previous + " current: " + current);
        spriteRenderer.sprite = pawnContainer.GetPawnByName(pawnName.Value).spritePawn;
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

    //[ServerRpc]
    //void StartGameServerRpc()
    //{
        
    //}

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
            {
                GameManager.instance.RegisterPlayer(this);
                //if (!IsServer && IsOwner) //mudar aqui para iniciar com mais players
                //    StartGameWithTwoPlayersServerRpc();
            }
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
