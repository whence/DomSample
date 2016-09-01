using System;
using System.Collections.Generic;
using System.IO;
using DomSample.Utils;

namespace DomSample.GameObjects
{
    public class Player
    {
        #region fields
        private readonly string name;

        private readonly List<Card> deckCards;
        private readonly List<Card> handCards;
        private readonly List<Card> playedCards;
        private readonly List<Card> discardCards;
        private readonly List<Card> revealCards;

        private int actionCountThisRound;
        private int buyCountThisRound;
        private int coinOffsetThisRound;
        #endregion

        #region properties
        public string Name
        {
            get { return name; }
        }

        public int HandCardsCount
        {
            get { return handCards.Count; }
        }

        public int ActiveCount { get; private set; }

        public IEnumerable<Card> DeckCards
        {
            get { return deckCards; }
        }

        public IEnumerable<Card> HandCards
        {
            get { return handCards; }
        }

        public IEnumerable<Card> PlayedCards
        {
            get { return playedCards; }
        }

        public IEnumerable<Card> DiscardCards
        {
            get { return discardCards; }
        }

        public IEnumerable<Card> RevealCards
        {
            get { return revealCards; }
        }

        public int ActionCountThisRound
        {
            get { return actionCountThisRound; }
        }

        public int BuyCountThisRound
        {
            get { return buyCountThisRound; }
        }

        public int CoinCountThisRound
        {
            get { return CalculateCoinsOnHand() + coinOffsetThisRound; }
        }

        public virtual PlayerType PlayerType
        {
            get { return PlayerType.Human; }
        }
        #endregion

        #region constructors
        public Player(string name, IEnumerable<Card> initialDeckCards)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");
            
            this.name = name;

            deckCards = new List<Card>(10);
            handCards = new List<Card>(5);
            playedCards = new List<Card>(5);
            discardCards = new List<Card>(5);
            revealCards = new List<Card>();

            deckCards.AddRange(initialDeckCards);
            deckCards.Shuffle(5);

            Cleanup();
        }
        #endregion

        #region game methods
        public void BecomeActive()
        {
            ActiveCount++;
        }

        public void DrawCards(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (deckCards.Count == 0)
                {
                    if (discardCards.Count == 0)
                        return;

                    deckCards.AddRange(discardCards);
                    discardCards.Clear();
                    deckCards.Shuffle(3);
                }

                handCards.Add(deckCards[deckCards.Count - 1]);
                deckCards.RemoveAt(deckCards.Count - 1);
            }
        }

        public Card DrawOneCard()
        {
            if (deckCards.Count == 0)
            {
                if (discardCards.Count == 0)
                    return null;

                deckCards.AddRange(discardCards);
                discardCards.Clear();
                deckCards.Shuffle(3);
            }

            handCards.Add(deckCards[deckCards.Count - 1]);
            deckCards.Remove(deckCards[deckCards.Count - 1]);

            return handCards[handCards.Count - 1];
        }

        public void AddAction(int count)
        {
            actionCountThisRound += count;
        }

        public bool CanAction()
        {
            return (actionCountThisRound > 0);
        }

        public Card FindCardOnHand(string cardName)
        {
            return Card.FindCard(cardName, handCards);
        }

        public Card[] FindCardsOnHand(IEnumerable<string> cardNames)
        {
            return Card.FindCards(cardNames, handCards);
        }

        public void PlayActionCard(Card card)
        {
            playedCards.Add(card);
            handCards.Remove(card);
            actionCountThisRound--;
        }

        public void PlayCard(Card card)
        {
            playedCards.Add(card);
            handCards.Remove(card);
        }

        public bool IsCardInPlay(Card card)
        {
            return playedCards.Contains(card);
        }

        public void AddBuy(int count)
        {
            buyCountThisRound += count;
        }

        public bool CanBuy()
        {
            return (buyCountThisRound > 0);
        }

        public bool HasCoins(int coins)
        {
            return (CoinCountThisRound >= coins);
        }

        public bool HasVictoryCardOnHand()
        {
            foreach (var card in handCards)
            {
                if (card.Info.IsVictoryCard)
                    return true;
            }
            return false;
        }

        public bool HasTreasureCardOnHand()
        {
            foreach (var card in handCards)
            {
                if (card.Info.IsTreasureCard)
                    return true;
            }
            return false;
        }

        public bool HasActionCardOnHand()
        {
            foreach (var card in handCards)
            {
                if (card.Info.IsActionCard)
                    return true;
            }
            return false;
        }

        public Card FindDefendCardOnHand()
        {
            return handCards.Find(x => x.Info.IsDefendCard);
        }

        public void BuyCard(Card card)
        {
            coinOffsetThisRound -= card.Info.Cost;
            
            GainCard(card);
            buyCountThisRound--;

            actionCountThisRound = 0;
        }

        public void GainCard(Card card)
        {
            discardCards.Add(card);
        }

        public void GainCards(Card[] cards)
        {
            discardCards.AddRange(cards);
        }

        public void GainCardOnHand(Card card)
        {
            handCards.Add(card);
        }

        public void GainCardOnDeck(Card card)
        {
            deckCards.Add(card);
        }

        public void AddCoinBonus(int coins)
        {
            coinOffsetThisRound += coins;
        }

        public void DiscardHandCards(Card[] cards)
        {
            discardCards.AddRange(cards);
            handCards.RemoveRange(cards);
        }

        public void DiscardAllHandCards()
        {
            if (handCards.Count > 0)
            {
                discardCards.AddRange(handCards);
                handCards.Clear();
            }
        }

        public void DiscardAllPlayedCards()
        {
            if (playedCards.Count > 0)
            {
                discardCards.AddRange(playedCards);
                playedCards.Clear();
            }
        }

        public void DiscardRevealCard(Card card)
        {
            discardCards.Add(card);
            revealCards.Remove(card);
        }

        public void DiscardRevealCards(Card[] cards)
        {
            discardCards.AddRange(cards);
            revealCards.RemoveRange(cards);
        }

        public void DiscardAllRevealCards()
        {
            if (revealCards.Count > 0)
            {
                discardCards.AddRange(revealCards);
                revealCards.Clear();
            }
        }

        public void DiscardDeckTopDown()
        {
            if (deckCards.Count > 0)
            {
                deckCards.Reverse();
                discardCards.AddRange(deckCards);
                deckCards.Clear();
            }
        }

        public void TrashHandCard(Card card, ICollection<Card> trashCards)
        {
            handCards.Remove(card);
            trashCards.Add(card);
        }

        public void TrashHandCards(Card[] cards, List<Card> trashCards)
        {
            handCards.RemoveRange(cards);
            trashCards.AddRange(cards);
        }

        public void TrashPlayedCard(Card card, ICollection<Card> trashCards)
        {
            playedCards.Remove(card);
            trashCards.Add(card);
        }

        public void TrashRevealCard(Card card, ICollection<Card> trashCards)
        {
            revealCards.Remove(card);
            trashCards.Add(card);
        }

        public Card RevealTopCardFromDeck()
        {
            if (deckCards.Count == 0)
            {
                if (discardCards.Count == 0)
                    return null;

                deckCards.AddRange(discardCards);
                discardCards.Clear();
                deckCards.Shuffle(3);
            }

            revealCards.Add(deckCards[deckCards.Count - 1]);
            deckCards.RemoveAt(deckCards.Count - 1);

            return revealCards[revealCards.Count - 1];
        }

        public void RevealHandCard(Card card)
        {
            revealCards.Add(card);
            handCards.Remove(card);
        }

        public Card[] RevealAllHandCards()
        {
            var cards = handCards.ToArray();

            revealCards.AddRange(cards);
            handCards.Clear();

            return cards;
        }

        public void UnrevealCardToHand(Card card)
        {
            handCards.Add(card);
            revealCards.Remove(card);
        }

        public void UnrevealCardsToHand(Card[] cards)
        {
            handCards.AddRange(cards);
            revealCards.RemoveRange(cards);
        }

        public void UnrevealCardToTopDeck(Card card)
        {
            deckCards.Add(card);
            revealCards.Remove(card);
        }

        public IEnumerable<Card> GetAllPlayerCards()
        {
            List<Card> cards = new List<Card>();
            cards.AddRange(HandCards);
            cards.AddRange(DiscardCards);
            cards.AddRange(DeckCards);
            cards.AddRange(RevealCards);
            cards.AddRange(PlayedCards);
            return cards;
        }

        public void Cleanup()
        {
            buyCountThisRound = 1;
            actionCountThisRound = 1;
            coinOffsetThisRound = 0;

            DiscardAllHandCards();
            DiscardAllPlayedCards();
            DiscardAllRevealCards();

            DrawCards(5);
        }

        public int CalculateTotalVictoryPoints()
        {
            int totalVictoryPoints = 0;

            int totalGardenCardCount = 0;
            int totalCardCount = 0;

            var cardCollections = new[] { deckCards, handCards, playedCards, discardCards, revealCards, };
            foreach (var cardCollection in cardCollections)
            {
                foreach (var card in cardCollection)
                {
                    totalVictoryPoints += card.Info.VictoriyPoints;

                    if ("garden".Equals(card.Info.CardName, StringComparison.OrdinalIgnoreCase))
                    {
                        totalGardenCardCount++;
                    }
                    totalCardCount++;
                }
            }
            totalVictoryPoints += (int)(Math.Floor((double)totalCardCount / 10) * totalGardenCardCount);

            return totalVictoryPoints;
        }
        #endregion

        #region output methods
        public void DisplayHeader(TextWriter writer)
        {
            writer.Write(this.Name);

            writer.Write(Chars.Space);
            writer.Write("T:");
            writer.Write(ActiveCount);

            writer.Write(Chars.Space);
            writer.Write("D:");
            writer.Write(deckCards.Count);

            writer.Write(Chars.Space);
            writer.Write("A:");
            writer.Write(actionCountThisRound);

            writer.Write(Chars.Space);
            writer.Write("B:");
            writer.Write(buyCountThisRound);

            writer.Write(Chars.Space);
            writer.Write("C:");
            writer.Write(CoinCountThisRound);
        }

        public void DisplayInfo(TextWriter writer)
        {
            writer.WriteLine("Hand Cards:");
            foreach (var card in handCards)
            {
                if (card.Info.IsActionCard)
                {
                    writer.Write('A');
                }
                else
                {
                    writer.Write('-');
                }
                writer.Write(Chars.Space);
                writer.WriteLine(card.Info.CardName);
            }

            if (playedCards.Count > 0)
            {
                writer.WriteLine("Played Cards:");
                foreach (var card in playedCards)
                {
                    writer.Write("- ");
                    writer.WriteLine(card.Info.CardName);
                }
            }
        }

        public void DisplayRevealCards(TextWriter writer)
        {
            writer.Write(this.Name);
            writer.Write(" revealed cards:");
            foreach (var card in revealCards)
            {
                writer.Write(card.Info.CardName);
                writer.Write(", ");
            }
            writer.WriteLine();
        }

        /// <summary>
        /// This is cheating. use it for debug only
        /// </summary>
        /// <param name="writer"></param>
        public void DisplayDeck(TextWriter writer)
        {
            writer.WriteLine("Deck Cards (top to bottom):");
            
            var deckCardsClone = new List<Card>(deckCards.Count);
            deckCardsClone.AddRange(deckCards);
            deckCardsClone.Reverse();

            foreach (var card in deckCardsClone)
            {
                writer.Write("- ");
                writer.WriteLine(card.Info.CardName);
            }
        }
        #endregion

        #region helper methods
        private int CalculateCoinsOnHand()
        {
            int coinsOnHand = 0;
            foreach (var card in handCards)
            {
                coinsOnHand += card.Info.Coins;
            }
            return coinsOnHand;
        }
        #endregion

        #region ai methods
        public virtual Instruction GenerateNextInstruction(IGame game)
        {
            return GeneralAIHelper.PerformAI(game, this);
        }
        #endregion
    }
}
