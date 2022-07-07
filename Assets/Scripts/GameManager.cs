using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.UI;
using MLAPI;

public class GameManager : MonoBehaviour
{
    //Singleton
    public static GameManager instance;

    //Inspector references
    [SerializeField]
    GameObject prefabUIManager;
    [SerializeField]
    GameEvent eventShowCardToPlayer, eventAskForGuessConfirmation, eventDiscardCard, eventUpdateStatusPanel, eventShowExtraCard, eventSendExtraCardToAskIfWantToGoToPlaceScreen;
    [SerializeField]
    CardContainer peopleCardContainer = null, 
        practicesCardContainer = null, 
        placesCardContainer = null, 
        extraCardsContainer = null;
    [SerializeField]
    PawnContainer pawnsContainer = null;
    [SerializeField]
    GameObject boardGO = null;

    //Runtime fields
    List<Player> listPlayers = new List<Player>();
    Card[] envelope = new Card[3];
    Card currentPlaceGuess, currentPersonGuess, currentPracticeGuess;
    int turnPlayer = 0;
    int showCardPlayer;
    bool registerPlayerLock = false;
    bool playerCanInteract = false;
    bool onGuessScreen = false;
    public Pawn clientPawn;

    //Properties
    public int _EnvelopePerson { get => envelope[0].id; }
    public int _EnvelopePractice { get => envelope[1].id; }
    public int _EnvelopePlace { get => envelope[2].id; }
    public Card _EnvelopePersonCard { get => envelope[0]; }
    public Card _EnvelopePracticeCard { get => envelope[1]; }
    public Card _EnvelopePlaceCard { get => envelope[2]; }
    public bool _RegisterPlayerLock { get => registerPlayerLock; }
    public bool _PlayerCanInteract { get => playerCanInteract; }
    public Card _CurrentPlaceGuess { get => currentPlaceGuess; }
    public Card _CurrentPersonGuess { get => currentPersonGuess; }
    public Card _CurrentPracticeGuess { get => currentPracticeGuess; }

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            Instantiate(prefabUIManager);
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    void Start()
    {
        //UIManager.instance.RequestScreen("Notes", true);
        //UIManager.instance.RequestScreen("Status Panel", true);
        UIManager.instance.RequestScreen("Room Manager Screen", true);
        //boardGO.SetActive(false);
    }

    public void FillEnvelope(int person, int practice, int place)
    {
        envelope[0] = peopleCardContainer._Cards.SingleOrDefault(obj => obj.id == person);
        envelope[1] = practicesCardContainer._Cards.SingleOrDefault(obj => obj.id == practice);
        envelope[2] = placesCardContainer._Cards.SingleOrDefault(obj => obj.id == place);
    }

    public Pawn GetPawnByName(string pawn)
    {
        foreach(Pawn p in pawnsContainer._Pawns)
        {
            if(p.name == pawn)
                return p;
        }
        return null;
    }

    public Card GetCardByTypeAndID(CardType type, int id)
    {
        if (type == CardType.person)
            return GetCardFromContainerByID(peopleCardContainer, id);
        else if (type == CardType.place)
            return GetCardFromContainerByID(placesCardContainer, id);
        else if (type == CardType.practice)
            return GetCardFromContainerByID(practicesCardContainer, id);
        else
            return GetCardFromContainerByID(extraCardsContainer, id);
    }

    Card GetCardFromContainerByID(CardContainer cardContainer, int id)
    {
        foreach (Card c in cardContainer._Cards)
        {
            if (c.id == id)
                return c;
        }
        return null;
    }

    public Card GetCardByID(int id) //REVER ESTA FUNÇÃO POIS CARDS DE DIFERENTES TIPOS PODEM TER O MESMO ID!!! Atualmente botei um id pra cada, faz mais sentido
    {
        foreach (Card c in peopleCardContainer._Cards)
        {
            if (c.id == id)
                return c;
        }
        foreach (Card c in practicesCardContainer._Cards)
        {
            if (c.id == id)
                return c;
        }
        foreach (Card c in placesCardContainer._Cards)
        {
            if (c.id == id)
                return c;
        }
        return null;
    }

    List<T> ShuffleList<T>(List<T> list)
    {
        return list.OrderBy(x => Random.value).ToList();
    }

    void DistributeCardsToPlayers(List<Card> cards)
    {
        List<Card> tempCards = new List<Card>(cards);
        foreach (Card c in envelope)
        {
            if (tempCards.Contains(c))
                tempCards.Remove(c);
        }
        while (tempCards.Count > 0)
        {
            foreach (Player p in listPlayers)
            {
                if (tempCards.Count > 0)
                {
                    int index = Random.Range(0, tempCards.Count);
                    p.AddToCardLists(tempCards[index]);
                    tempCards.RemoveAt(index);
                }
            }
        }
    }

    //essa função só serve para retornar para GuessScreenController, o(s) Player(s) poderiam retornar diretamente em vez de usar um intermediário
    public List<Card> GetPersonCards() //quando o jogador puder selecionar quais cartas mostrar, pode repetir cartas já mostradas, é responsabilidade do jogador do turno cuidar para não pedir as mesmas (?)
    {
        List<Card> personCards = new List<Card>();
        
        foreach(Player p in listPlayers)
        {
            if(p.id != turnPlayer)
            {
                foreach(Card c in p._CardPersonList)
                {
                    if (!listPlayers[turnPlayer].IsItADiscardedCard(c))
                        personCards.Add(c);
                }
            }
        }

        foreach(Card c in envelope)
        {
            if(c.type == CardType.person)
            {
                personCards.Add(c);
                return personCards;
            }
        }

        return personCards;
    }

    //essa função só serve para retornar para GuessScreenController, o(s) Player(s) poderiam retornar diretamente em vez de usar um intermediário
    public List<Card> GetPracticeCards()
    {
        List<Card> practiceCards = new List<Card>();

        foreach (Player p in listPlayers)
        {
            if (p.id != turnPlayer)
            {
                foreach (Card c in p._CardPracticeList)
                {
                    if (!listPlayers[turnPlayer].IsItADiscardedCard(c))
                        practiceCards.Add(c);
                }
            }
        }

        foreach (Card c in envelope)
        {
            if (c.type == CardType.practice)
            {
                practiceCards.Add(c);
                return practiceCards;
            }
        }

        return practiceCards;
    }

    public void RegisterPlayer(Player player)
    {
        if (!registerPlayerLock)
        {
            registerPlayerLock = true;
            player.id = listPlayers.Count;
            listPlayers.Add(player); //precisa gerenciar quando um jogador desconecta, provavelmente usar delegate para decadastrar
            
            if (listPlayers.Count == 1)
                playerCanInteract = true;
            else if(listPlayers.Count == 2 && NetworkManager.Singleton.IsServer)
                OrganizeCards();

            registerPlayerLock = false;
        }
    }

    void OrganizeCards()
    {

        //foreach (MLAPI.Connection.NetworkClient c in NetworkManager.Singleton.ConnectedClientsList)
        //{
        //    Debug.Log("Client " + c.ClientId + " with PlayerObject " + c.PlayerObject + " has " + c.OwnedObjects.Count + " objects");
        //}

        peopleCardContainer._Cards = ShuffleList(peopleCardContainer._Cards);
        practicesCardContainer._Cards = ShuffleList(practicesCardContainer._Cards);
        placesCardContainer._Cards = ShuffleList(placesCardContainer._Cards);

        envelope = new Card[3];
        envelope[0] = peopleCardContainer._Cards[0];
        envelope[1] = practicesCardContainer._Cards[0];
        envelope[2] = placesCardContainer._Cards[0];

        //Distribute people, practices and places cards to players
        DistributeCardsToPlayers(peopleCardContainer._Cards);
        DistributeCardsToPlayers(practicesCardContainer._Cards);
        DistributeCardsToPlayers(placesCardContainer._Cards);

        SendDiscardCardEvents(listPlayers[0]); //apenas para o jogador 1 do server
        listPlayers[1].RequestPlayerToSendDiscardCardEvents();
    }

    public void SendDiscardCardEvents(Player player)
    {
        //COMEÇO GAMBIARRA
        UIManager.instance.RequestScreen("Notes", true); 
        //FIM GAMBIARRA

        foreach (Card cPlace in player._CardPlaceList)
            eventDiscardCard.Raise(cPlace);
        
        foreach (Card cPerson in player._CardPersonList)
            eventDiscardCard.Raise(cPerson);
        
        foreach (Card cPractice in player._CardPracticeList)
            eventDiscardCard.Raise(cPractice);

        //COMEÇO GAMBIARRA
        UIManager.instance.RequestScreen("Notes", false);
        //FIM GAMBIARRA
    }

    void ChangePlayerTurn()
    {
        listPlayers[turnPlayer].isMyTurn.Value = false; //mudança ocorre duas vezes porque é uma NetworkVariable
        
        if (turnPlayer < listPlayers.Count - 1)
            turnPlayer++;
        else
            turnPlayer = 0;

        listPlayers[turnPlayer].isMyTurn.Value = true; //mudança ocorre duas vezes porque é uma NetworkVariable            
        eventUpdateStatusPanel.Raise(listPlayers[turnPlayer].IsOwner);
    }

    public void OnRequestExtraCard()
    {
        UIManager.instance.RequestScreen("Extra Card Screen", true);
        //get random extra card from list and emit it to ExtraCardScreenController
        eventShowExtraCard.Raise(extraCardsContainer._Cards[Random.Range(0, extraCardsContainer._Cards.Count)]);
    }

    public void OnAskIfWantToGuess(Card cardPlace)
    {
        currentPlaceGuess = cardPlace;
        playerCanInteract = false;
        onGuessScreen = true;
        UIManager.instance.RequestScreen("Guess Screen", true);
    }

    public void OnCloseGuessScreen()
    {
        UIManager.instance.RequestScreen("Guess Screen", false);
        playerCanInteract = true;
        onGuessScreen = false;
    }

    public void CloseExtraCardScreen()
    {
        UIManager.instance.RequestScreen("Extra Card Screen", false);
    }

    public void OnChangePlayerTurn()
    {
        Debug.Log("Called OnChangePlayerTurn");
        ChangePlayerTurn();
    }

    List<Card> CheckGuessCards(Card placeCard, Card personCard, Card practiceCard)
    {
        List<Card> cardsToShow = new List<Card>();
        foreach (Player p in listPlayers)
        {
            if (p.id != turnPlayer)
            {
                if(placeCard != null)
                {
                    foreach (Card c in p._CardPlaceList)
                    {
                        if (c == placeCard)
                        {
                            if (!listPlayers[turnPlayer].IsItADiscardedCard(c))
                                cardsToShow.Add(c);
                        }
                    }
                }

                if(personCard != null)
                {
                    foreach (Card c in p._CardPersonList)
                    {
                        if (c == personCard)
                        {
                            if (!listPlayers[turnPlayer].IsItADiscardedCard(c))
                                cardsToShow.Add(c);
                        }
                    }
                }
                
                if(practiceCard != null)
                {
                    foreach (Card c in p._CardPracticeList)
                    {
                        if (c == practiceCard)
                        {
                            if (!listPlayers[turnPlayer].IsItADiscardedCard(c))
                                cardsToShow.Add(c);
                        }
                    }
                }
                
                if (cardsToShow.Count > 0)
                {
                    showCardPlayer = p.id;
                    return cardsToShow;
                }
            }
        }
        return cardsToShow;
    }

    public void SendGuessToOtherPlayers(Card placeCard, Card personCard, Card practiceCard)
    {
        currentPersonGuess = personCard;
        currentPracticeGuess = practiceCard;
        List<Card> cardsToShow = CheckGuessCards(placeCard, personCard, practiceCard);
        if(cardsToShow.Count == 0)
            eventAskForGuessConfirmation.Raise(); //ninguém tinha as cartas, pergunta para o jogador do turno se quer confirmar o palpite
        else
        {
            //IMPORTANTE: por enquanto fica a primeira carta encontrada, mas o certo é programar uma interface para o outro jogador escolher qual carta mostrar!!!
            eventShowCardToPlayer.Raise(cardsToShow[0], showCardPlayer);
            listPlayers[turnPlayer].AddToDiscardedCards(cardsToShow[0]);
            //if(turnPlayer == 0) //MUDAR ISSO QUANDO TIVER SE REFERINDO APENAS AO JOGADOR DO PC
            //{
            eventDiscardCard.Raise(cardsToShow[0]);
            //}
        }
    }
    
    public void OnTryToWinWithGuess()
    {
        if (!envelope.Contains(currentPlaceGuess) || !envelope.Contains(currentPersonGuess) || !envelope.Contains(currentPracticeGuess))
        {
            //lose
            UIManager.instance.RequestScreen("Lose Screen", true);
            //mandar evento para o outro jogador via rpc
            if (NetworkManager.Singleton.IsServer)
                listPlayers[turnPlayer].SendWinEventClientRpc();
            else
                listPlayers[turnPlayer].SendWinEventServerRpc();
        }
        else
        {
            //win
            UIManager.instance.RequestScreen("Win Screen", true);
            //mandar evento para o outro jogador via rpc
            if (NetworkManager.Singleton.IsServer)
                listPlayers[turnPlayer].SendLoseEventClientRpc();
            else
                listPlayers[turnPlayer].SendLoseEventServerRpc();
        }
    }

    public void OnEffectGoToPlaceOptional(ExtraCard extraCard)
    {
        CloseExtraCardScreen();
        UIManager.instance.RequestScreen("AskIfWantToGoToPlace Screen", true);
        eventSendExtraCardToAskIfWantToGoToPlaceScreen.Raise(extraCard);
    }

    public void OnCloseAskIfWantToGoToPlaceScreen()
    {
        UIManager.instance.RequestScreen("AskIfWantToGoToPlace Screen", false);
    }

    public void OnWin()
    {
        UIManager.instance.RequestScreen("Win Screen", true);
    }

    public void OnLose()
    {
        UIManager.instance.RequestScreen("Lose Screen", true);
    }

    public void OnCloseWinScreen()
    {
        UIManager.instance.RequestScreen("Win Screen", false);
    }

    public void OnCloseLoseScreen()
    {
        UIManager.instance.RequestScreen("Lose Screen", false);
    }

    public void OnPlayerCanInteract()
    {
        if (!onGuessScreen)
            playerCanInteract = true;
    }

    public void OnPlayerCannotInteract()
    {
        if (!onGuessScreen)
            playerCanInteract = false;
    }

    public void OnStartGame()
    {
        //UIManager.instance.RequestScreen("Room Manager Screen", false);
        UIManager.instance.RequestScreen("Status Panel", true);
        UIManager.instance.RequestScreen("Notes", true);
        boardGO.SetActive(true);
        eventUpdateStatusPanel.Raise(listPlayers[turnPlayer].IsOwner);
        listPlayers[turnPlayer].isMyTurn.Value = true;
    }
}