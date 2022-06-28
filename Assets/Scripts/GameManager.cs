using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
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
    public int currentDice1Result = 0;
    public int currentDice2Result = 0;
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

    void DistributeCards()
    {
        List<Card> people = new List<Card>(peopleCardContainer._Cards);
        foreach(Card c in envelope)
        {
            if (people.Contains(c))
                people.Remove(c);
        }
        while (people.Count > 0)
        {
            foreach (Player p in listPlayers)
            {
                if(people.Count > 0)
                {
                    int index = Random.Range(0, people.Count);
                    p.AddToCardLists(people[index]);
                    people.RemoveAt(index);
                }
            }
        }

        List<Card> practices = new List<Card>(practicesCardContainer._Cards);
        foreach (Card c in envelope)
        {
            if (practices.Contains(c))
                practices.Remove(c);
        }
        while (practices.Count > 0)
        {            
            foreach (Player p in listPlayers)
            {
                if (practices.Count > 0)
                {
                    int index = Random.Range(0, practices.Count);
                    p.AddToCardLists(practices[index]);
                    practices.RemoveAt(index);
                }
            }
        }

        List<Card> places = new List<Card>(placesCardContainer._Cards);
        foreach (Card c in envelope)
        {
            if (places.Contains(c))
                places.Remove(c);
        }
        while (places.Count > 0)
        {            
            foreach (Player p in listPlayers)
            {
                if (places.Count > 0)
                {
                    int index2 = Random.Range(0, places.Count);
                    p.AddToCardLists(places[index2]);
                    places.RemoveAt(index2);
                }
            }
        }
    }

    public List<Card> GetPersonCards() //quando o jogador puder selecionar quais cartas mostrar, pode repetir cartas já mostradas, é responsabilidade do jogador do turno cuidar para não pedir as mesmas (?)
    {
        List<Card> people = new List<Card>();
        foreach(Player p in listPlayers)
        {
            if(p.id != turnPlayer)
            {
                foreach(Card c in p.GetPersonCards())
                {
                    if (!listPlayers[turnPlayer].IsItADiscardedCard(c))
                        people.Add(c);
                }
            }
        }
        foreach(Card c in envelope)
        {
            if(c.type == CardType.person)
            {
                people.Add(c);
                return people;
            }
        }
        return people;
    } 

    //public List<Card> GetPlaceCards()
    //{
    //    return listPlaces;
    //}

    public List<Card> GetPracticeCards()
    {
        List<Card> practices = new List<Card>();
        foreach (Player p in listPlayers)
        {
            if (p.id != turnPlayer)
            {
                foreach (Card c in p.GetPracticeCards())
                {
                    if (!listPlayers[turnPlayer].IsItADiscardedCard(c))
                        practices.Add(c);
                }
            }
        }
        foreach (Card c in envelope)
        {
            if (c.type == CardType.practice)
            {
                practices.Add(c);
                return practices;
            }
        }
        return practices;
    }

    public void RegisterPlayer(Player player)
    {
        if (!registerPlayerLock)
        {
            registerPlayerLock = true;
            player.id = listPlayers.Count;
            listPlayers.Add(player); //precisa gerenciar quando um jogador desconecta, provavelmente usar delegate para decadastrar
            
            if (listPlayers.Count == 1)
            {
                player.isMyTurn.Value = true;
                playerCanInteract = true;
                if (player.IsOwner)
                    player.canRollDices = true;
            }
            else if(listPlayers.Count == 2 && NetworkManager.Singleton.IsServer)
            {
                if (peopleCardContainer != null)
                    peopleCardContainer._Cards = ShuffleList(peopleCardContainer._Cards);
                if (practicesCardContainer != null)
                    practicesCardContainer._Cards = ShuffleList(practicesCardContainer._Cards);
                if (placesCardContainer != null)
                    placesCardContainer._Cards = ShuffleList(placesCardContainer._Cards);

                envelope = new Card[3];
                envelope[0] = peopleCardContainer._Cards[0];
                envelope[1] = practicesCardContainer._Cards[0];
                envelope[2] = placesCardContainer._Cards[0];

                DistributeCards();
                SendDiscardCardEvents(listPlayers[0]); //apenas para o jogador 1 do server
                listPlayers[1].RequestPlayerToSendDiscardCardEvents();
            }
            registerPlayerLock = false;
        }
    }

    public void SendDiscardCardEvents(Player player)
    {
        //COMEÇO GAMBIARRA
        UIManager.instance.RequestScreen("Notes", true); 
        //FIM GAMBIARRA

        foreach (Card cPlace in player.GetPlaceCards())
            eventDiscardCard.Raise(cPlace);
        
        foreach (Card cPerson in player.GetPersonCards())
            eventDiscardCard.Raise(cPerson);
        
        foreach (Card cPractice in player.GetPracticeCards())
            eventDiscardCard.Raise(cPractice);

        //COMEÇO GAMBIARRA
        UIManager.instance.RequestScreen("Notes", false);
        //FIM GAMBIARRA
    }

    void ChangePlayerTurn()
    {
        listPlayers[turnPlayer].isMyTurn.Value = false;
        
        if (turnPlayer < listPlayers.Count - 1)
            turnPlayer++;
        else
            turnPlayer = 0;

        listPlayers[turnPlayer].isMyTurn.Value = true;
        bool isOwner = listPlayers[turnPlayer].IsOwner;
        
        if (isOwner)
            listPlayers[turnPlayer].canRollDices = true;
        
        eventUpdateStatusPanel.Raise(isOwner);
    }

    public void OnRequestExtraCard()
    {
        UIManager.instance.RequestScreen("Extra Card Screen", true);
        //get random extra card from list and emit it to ExtraCardScreenController
        eventShowExtraCard.Raise(extraCardsContainer._Cards[Random.Range(0, extraCardsContainer._Cards.Count)]);
    }

    public void OnDisplayDicesResults(int dice1, int dice2)
    {
        currentDice1Result = dice1;
        currentDice2Result = dice2;
        
        if(UIManager.instance.IsScreenVisible("Dices Results"))
            UIManager.instance.RequestScreen("Dices Results", false);
        
        UIManager.instance.RequestScreen("Dices Results", true);
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
        ChangePlayerTurn();
    }

    List<Card> CheckGuessCards(Card place, Card person, Card practice)
    {
        List<Card> cardsToShow = new List<Card>();
        foreach (Player p in listPlayers)
        {
            if (p.id != turnPlayer)
            {
                if(place != null)
                {
                    foreach (Card c in p.GetPlaceCards())
                    {
                        if (c == place)
                        {
                            if (!listPlayers[turnPlayer].IsItADiscardedCard(c))
                                cardsToShow.Add(c);
                        }
                    }
                }

                if(person != null)
                {
                    foreach (Card c in p.GetPersonCards())
                    {
                        if (c == person)
                        {
                            if (!listPlayers[turnPlayer].IsItADiscardedCard(c))
                                cardsToShow.Add(c);
                        }
                    }
                }
                
                if(practice != null)
                {
                    foreach (Card c in p.GetPracticeCards())
                    {
                        if (c == practice)
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

    public void SendGuessToOtherPlayers(Card place, Card person, Card practice)
    {
        currentPersonGuess = person;
        currentPracticeGuess = practice;
        List<Card> cardsToShow = CheckGuessCards(place, person, practice);
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
        UIManager.instance.RequestScreen("Room Manager Screen", false);
        UIManager.instance.RequestScreen("Status Panel", true);
        UIManager.instance.RequestScreen("Notes", true);
        boardGO.SetActive(true);
        eventUpdateStatusPanel.Raise(listPlayers[turnPlayer].IsOwner);
    }
}