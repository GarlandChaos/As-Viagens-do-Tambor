using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.UI;
using MLAPI;
using MLAPI.Connection;

[DisallowMultipleComponent]
public class GameManager : MonoBehaviour
{
    //Singleton
    public static GameManager instance = null;

    //Inspector references
    [SerializeField]
    GameObject prefabUIManager = null;
    [SerializeField]
    GameEvent eventShowCardToPlayer = null, eventAskForGuessConfirmation = null, eventShowExtraCard = null, eventSendExtraCardToAskIfWantToGoToPlaceScreen = null,
        eventRequestToCheckGuessCards = null, eventRequestToCheckEnvelope = null, eventWin = null, eventLose = null;
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
    Card[] envelope = new Card[3];
    Card currentPlaceGuess = null, currentPersonGuess = null, currentPracticeGuess = null;
    int turnPlayerIndex = 0;
    string unraveledCardPlayerName = string.Empty;
    bool playerCanInteract = false;
    bool onGuessScreen = false;
    public bool DEBUGROOM = true; //PROVISÓRIO
    public bool DEBUGEXTRACARD = false; //PROVISÓRIO

    //Properties
    public int _EnvelopePerson { get => envelope[0].id; }
    public int _EnvelopePractice { get => envelope[1].id; }
    public int _EnvelopePlace { get => envelope[2].id; }
    public Card _EnvelopePersonCard { get => envelope[0]; }
    public Card _EnvelopePracticeCard { get => envelope[1]; }
    public Card _EnvelopePlaceCard { get => envelope[2]; }
    public bool _PlayerCanInteract { get => playerCanInteract; }
    public Card _CurrentPlaceGuess { get => currentPlaceGuess; }
    public Card _CurrentPersonGuess { get => currentPersonGuess; }
    public Card _CurrentPracticeGuess { get => currentPracticeGuess; }
    public string _UnraveledCardPlayerName { get => unraveledCardPlayerName; }

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

    public void OnStartGame()
    {
        boardGO.SetActive(true);
        playerCanInteract = true;
        
        if (NetworkManager.Singleton.IsServer)
        {
            OrganizeCards();
            NetworkManager.Singleton.ConnectedClientsList[turnPlayerIndex].PlayerObject.GetComponent<Player>().isMyTurn.Value = true;
        }
    }

    public void ResetFields()
    {
        boardGO.SetActive(false);
        envelope[0] = null;
        envelope[1] = null;
        envelope[2] = null;
        currentPlaceGuess = null;
        currentPersonGuess = null;
        currentPracticeGuess = null;
        turnPlayerIndex = 0;
        unraveledCardPlayerName = string.Empty;
        playerCanInteract = false;
        onGuessScreen = false;
    }

    //public void FillEnvelope(int person, int practice, int place)
    //{
    //    envelope[0] = peopleCardContainer._Cards.SingleOrDefault(obj => obj.id == person);
    //    envelope[1] = practicesCardContainer._Cards.SingleOrDefault(obj => obj.id == practice);
    //    envelope[2] = placesCardContainer._Cards.SingleOrDefault(obj => obj.id == place);
    //}

    public Pawn GetPawnByName(string pawn)
    {
        foreach(Pawn p in pawnsContainer._Pawns)
            if(p.name == pawn)
                return p;

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
            if (c.id == id)
                return c;

        return null;
    }

    public Card GetCardByID(int id) //REVER ESTA FUNÇÃO POIS CARDS DE DIFERENTES TIPOS PODEM TER O MESMO ID!!! Atualmente botei um id pra cada, faz mais sentido
    {
        foreach (Card c in peopleCardContainer._Cards)
            if (c.id == id)
                return c;

        foreach (Card c in practicesCardContainer._Cards)
            if (c.id == id)
                return c;

        foreach (Card c in placesCardContainer._Cards)
            if (c.id == id)
                return c;

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
            if (tempCards.Contains(c))
                tempCards.Remove(c);

        while (tempCards.Count > 0)
        {
            foreach (NetworkClient n in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (tempCards.Count > 0)
                {
                    Player p = n.PlayerObject.GetComponent<Player>();
                    int index = Random.Range(0, tempCards.Count);
                    p.AddToCardLists(tempCards[index]);
                    tempCards.RemoveAt(index);
                }
            }
        }
    }

    List<Card> GetRemainingCardsByType(CardType type)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            List<Card> cards = new List<Card>();
            Player turnPlayer = NetworkManager.Singleton.ConnectedClientsList[turnPlayerIndex].PlayerObject.GetComponent<Player>();

            foreach (NetworkClient n in NetworkManager.Singleton.ConnectedClientsList)
            {
                Player p = n.PlayerObject.GetComponent<Player>();
                List<Card> cardTypeList = type == CardType.person ? p._CardPersonList : type == CardType.practice ? p._CardPracticeList : p._CardPlaceList;
                if (!p.isMyTurn.Value)
                {
                    foreach (Card c in cardTypeList)
                    {
                        if (!turnPlayer.IsItADiscardedCard(c))
                            cards.Add(c);
                    }
                }
            }

            foreach (Card c in envelope)
            {
                if (c.type == type)
                {
                    cards.Add(c);
                    return cards;
                }
            }

            return cards;
        }

        return null;
    }

    public List<Card> GetPersonCards()
    {
        return GetRemainingCardsByType(CardType.person);
    }

    public List<Card> GetPracticeCards()
    {
        return GetRemainingCardsByType(CardType.practice);
    }

    void OrganizeCards()
    {
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
    }

    void ChangePlayerTurn()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.ConnectedClientsList[turnPlayerIndex].PlayerObject.GetComponent<Player>().isMyTurn.Value = false;
            
            if (turnPlayerIndex < NetworkManager.Singleton.ConnectedClientsList.Count - 1) //tem como fazer isso com menos linhas!!!
                turnPlayerIndex++;
            else
                turnPlayerIndex = 0;

            NetworkManager.Singleton.ConnectedClientsList[turnPlayerIndex].PlayerObject.GetComponent<Player>().isMyTurn.Value = true;
        }
    }

    public void OnRequestExtraCard()
    {
        eventShowExtraCard.Raise(extraCardsContainer._Cards[Random.Range(0, extraCardsContainer._Cards.Count)]);
    }

    public void OnAskIfWantToGuess(Card cardPlace)
    {
        currentPlaceGuess = cardPlace;
        playerCanInteract = false;
        onGuessScreen = true;
    }

    public void OnCloseGuessScreen()
    {
        playerCanInteract = true;
        onGuessScreen = false;
    }

    public void OnChangePlayerTurn()
    {
        ChangePlayerTurn();
    }

    public List<Card> CheckGuessCards(Card placeCard, Card personCard, Card practiceCard)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            List<Card> cardsToShow = new List<Card>();
            Player turnPlayer = NetworkManager.Singleton.ConnectedClientsList[turnPlayerIndex].PlayerObject.GetComponent<Player>();

            foreach (NetworkClient n in NetworkManager.Singleton.ConnectedClientsList)
            {
                Player p = n.PlayerObject.GetComponent<Player>();
                if (!p.isMyTurn.Value)
                {
                    AddToCardListIfAnotherPlayerContains(placeCard, cardsToShow, turnPlayer, p);
                    AddToCardListIfAnotherPlayerContains(personCard, cardsToShow, turnPlayer, p);
                    AddToCardListIfAnotherPlayerContains(practiceCard, cardsToShow, turnPlayer, p);
                }
            }

            return cardsToShow;
        }

        return null;
    }

    void AddToCardListIfAnotherPlayerContains(Card cardToCheck, List<Card> cardList, Player turnPlayer, Player otherPlayer)
    {
        List<Card> cardTypeList = cardToCheck.type == CardType.person ? otherPlayer._CardPersonList : cardToCheck.type == CardType.practice ? otherPlayer._CardPracticeList : otherPlayer._CardPlaceList;
        foreach (Card c in cardTypeList)
        {
            if (c == cardToCheck)
            {
                if (!turnPlayer.IsItADiscardedCard(c))
                {
                    cardList.Add(c);
                    if (cardList.Count == 1)
                        unraveledCardPlayerName = otherPlayer.playerName.Value;
                }
            }
        }
    }

    public void SetGuessCardsAndCheck(Card personCard, Card practiceCard)
    {
        currentPersonGuess = personCard;
        currentPracticeGuess = practiceCard;
        List<Card> cardsToShow = CheckGuessCards(currentPlaceGuess, personCard, practiceCard);

        if(cardsToShow != null) //is host
        {
            if (cardsToShow.Count == 0)
                eventAskForGuessConfirmation.Raise(); //ninguém tinha as cartas, pergunta para o jogador do turno se quer confirmar o palpite
            else
            {
                eventShowCardToPlayer.Raise(cardsToShow[0], unraveledCardPlayerName);
                NetworkManager.Singleton.ConnectedClientsList[turnPlayerIndex].PlayerObject.GetComponent<Player>().AddToDiscardedCards(cardsToShow[0]);
            }
        }
        else //is client
            eventRequestToCheckGuessCards.Raise(currentPlaceGuess.id, currentPersonGuess.id, currentPracticeGuess.id);
    }
    
    public bool CheckEnvelope(Card placeCard, Card personCard, Card practiceCard)
    {
        return envelope.Contains(placeCard) && envelope.Contains(personCard) && envelope.Contains(practiceCard);
    }

    public void OnTryToWinWithGuess()
    {
        //Checa se é servidor
        //Se for servidor:
        //  checa envelope
        //      se ganhou, manda ClientRPC para todos encerrando o jogo
        //      se perdeu, manda ClientRPC e assiste o jogo até o fim
        //Se for cliente:
        //  manda ServerRPC para checar o envelope
        //  servidor checa e manda resposta para o cliente
        //      se venceu, manda ServerRPC para encerrar o jogo
        //      se perdeu, manda ServerRPC e assiste o jogo

        if (NetworkManager.Singleton.IsServer) //is server
        {
            if (CheckEnvelope(currentPlaceGuess, currentPersonGuess, currentPracticeGuess)) //local player win
                eventWin.Raise(); //manda ClientRPC para todos encerrando o jogo
            else //local player lose
                eventLose.Raise(); //manda ClientRPC e assiste o jogo até o fim
        }
        else //is client
        {
            //  manda ServerRPC para checar o envelope
            eventRequestToCheckEnvelope.Raise(currentPlaceGuess.id, currentPersonGuess.id, currentPracticeGuess.id);
            //  servidor checa e manda resposta para o cliente
            //      se venceu, manda ServerRPC para encerrar o jogo
            //      se perdeu, manda ServerRPC e assiste o jogo
        }
    }

    public void OnEffectGoToPlaceOptional(ExtraCard extraCard)
    {
        eventSendExtraCardToAskIfWantToGoToPlaceScreen.Raise(extraCard);
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
}