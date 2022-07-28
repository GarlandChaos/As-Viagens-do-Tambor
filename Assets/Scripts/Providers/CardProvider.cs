using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Connection;
using System.Linq;

public class CardProvider : MonoBehaviour
{
    //Singleton
    public static CardProvider instance = null;

    //Inspector reference fields
    [SerializeField]
    CardContainer peopleCardContainer = null,
        practicesCardContainer = null,
        placesCardContainer = null,
        extraCardsContainer = null;

    //Runtime fields
    Card[] envelope = new Card[3];

    //Properties
    public int _EnvelopePerson { get => envelope[0].id; }
    public int _EnvelopePractice { get => envelope[1].id; }
    public int _EnvelopePlace { get => envelope[2].id; }
    public Card _EnvelopePersonCard { get => envelope[0]; }
    public Card _EnvelopePracticeCard { get => envelope[1]; }
    public Card _EnvelopePlaceCard { get => envelope[2]; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
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

    List<Card> GetRemainingCardsByType(CardType type)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            List<Card> cards = new List<Card>();
            Player turnPlayer = NetworkManager.Singleton.ConnectedClientsList[GameManager.instance._TurnPlayerIndex].PlayerObject.GetComponent<Player>();

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

    public void OrganizeCards()
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

    public bool CheckEnvelope(Card placeCard, Card personCard, Card practiceCard)
    {
        return envelope.Contains(placeCard) && envelope.Contains(personCard) && envelope.Contains(practiceCard);
    }

    public void ClearEnvelope(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            envelope[0] = null;
            envelope[1] = null;
            envelope[2] = null;
        }
    }

    public List<Card> CheckGuessCards(Card placeCard, Card personCard, Card practiceCard)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            List<Card> cardsToShow = new List<Card>();
            Player turnPlayer = NetworkManager.Singleton.ConnectedClientsList[GameManager.instance._TurnPlayerIndex].PlayerObject.GetComponent<Player>();

            foreach (NetworkClient n in NetworkManager.Singleton.ConnectedClientsList)
            {
                Player p = n.PlayerObject.GetComponent<Player>();
                if (!p.isMyTurn.Value)
                {
                    if (placeCard != null)
                        AddToCardListIfAnotherPlayerContains(placeCard, cardsToShow, turnPlayer, p);

                    if (personCard != null)
                        AddToCardListIfAnotherPlayerContains(personCard, cardsToShow, turnPlayer, p);

                    if (practiceCard != null)
                        AddToCardListIfAnotherPlayerContains(practiceCard, cardsToShow, turnPlayer, p);
                }
            }

            return cardsToShow;
        }

        return null;
    }

    void AddToCardListIfAnotherPlayerContains(Card cardToCheck, List<Card> cardListToAdd, Player turnPlayer, Player otherPlayer)
    {
        List<Card> cardTypeList = cardToCheck.type == CardType.person ? otherPlayer._CardPersonList : cardToCheck.type == CardType.practice ? otherPlayer._CardPracticeList : otherPlayer._CardPlaceList;
        foreach (Card c in cardTypeList)
        {
            if (c == cardToCheck)
            {
                if (!turnPlayer.IsItADiscardedCard(c))
                {
                    cardListToAdd.Add(c);
                    if (cardListToAdd.Count == 1)
                        GameManager.instance.unraveledCardPlayerName = otherPlayer.playerName.Value;
                }
            }
        }
    }
}
