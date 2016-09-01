using System;
using System.Collections.Generic;
using System.IO;
using DomSample.Utils;

namespace DomSample.GameObjects
{
    public class Supply
    {
        #region fields
        private readonly Dictionary<string, CardPile> cardPiles;
        #endregion

        #region properties
        public IDictionary<string, CardPile> CardPiles
        {
            get { return cardPiles; }
        }
        #endregion

        #region constructors
        public Supply(int playerCount, ICollection<string> kingdomCardNames)
        {
            cardPiles = new Dictionary<string, CardPile>(20, StringComparer.OrdinalIgnoreCase);
            BuildCardPiles(playerCount, kingdomCardNames);
        }
        #endregion

        #region public methods
        public Card GetCard(ICardInfo cardInfo)
        {
            if (cardInfo == null)
                throw new ArgumentNullException("cardInfo");

            CardPile cardPile;
            if (!cardPiles.TryGetValue(cardInfo.CardName, out cardPile))
                return null;

            if (cardPile.RemoveCards(1) == 0)
                return null;

            return new Card(cardInfo);
        }

        public Card[] GetCards(ICardInfo cardInfo, int count)
        {
            if (cardInfo == null)
                throw new ArgumentNullException("cardInfo");

            if (count <= 0)
                throw new ArgumentOutOfRangeException("count", "Must be positive");

            CardPile cardPile;
            if (!cardPiles.TryGetValue(cardInfo.CardName, out cardPile))
                return new Card[0];

            count = cardPile.RemoveCards(count);

            var cards = new Card[count];
            for (int i = 0; i < count; i++)
            {
                cards[i] = new Card(cardInfo);
            }
            return cards;
        }

        public bool IsDry()
        {
            if (cardPiles["Province"].IsEmpty)
                return true;

            int emptyPileCount = 0;
            foreach (var pair in cardPiles)
            {
                if (pair.Value.IsEmpty)
                    emptyPileCount++;
            }
            return (emptyPileCount >= 3);
        }

        public void Display(TextWriter writer)
        {
            var cardInfos = cardPiles.ToKeyArray(CardCentral.Shared.GetCardInfo);
            Array.Sort(cardInfos, CompareCardInfos);

            foreach (var cardInfo in cardInfos)
            {
                var cardPile = cardPiles[cardInfo.CardName];

                if (cardPile.CardCount < 10)
                {
                    writer.Write(Chars.Space);
                }
                writer.Write(cardPile.CardCount);
                writer.Write(Chars.Space);

                writer.Write(cardInfo.Cost);
                writer.Write(Chars.Space);

                writer.Write(cardInfo.CardName);
                writer.WriteLine();
            }
        }
        #endregion

        #region private methods
        private void BuildCardPiles(int playerCount, ICollection<string> kingdomCardNames)
        {
            if (playerCount < 1 || playerCount > 4)
                throw new ArgumentException("must be between 1 to 4 players", "playerCount");
            
            // kingdom cards
            kingdomCardNames = new HashSet<string>(kingdomCardNames, StringComparer.OrdinalIgnoreCase);
            if (kingdomCardNames.Count != 10)
                throw new ArgumentException("must have 10 different kingdom cards", "kingdomCardNames");

            foreach (var cardName in kingdomCardNames)
            {
                var cardInfo = CardCentral.Shared.GetCardInfo(cardName);
                if (cardInfo == null)
                    throw new ArgumentException("unknown card:" + cardName);

                if (!cardInfo.IsKingdomCard)
                    throw new ArgumentException(cardName + " is not a kingdom card");

                if (cardInfo.IsVictoryCard)
                {
                    switch (playerCount)
                    {
                        case 1:
                        case 2:
                            AddCards(cardInfo.CardName, 8);
                            break;

                        case 3:
                        case 4:
                            AddCards(cardInfo.CardName, 12);
                            break;
                    }
                }
                else
                {
                    AddCards(cardInfo.CardName, 10);
                }
            }

            // treasure cards
            AddCards("Copper", 60);
            AddCards("Silver", 40);
            AddCards("Gold", 30);

            // victory cards
            switch (playerCount)
            {
                case 1:
                case 2:
                    AddCards("Estate", 8);
                    AddCards("Duchy", 8);
                    AddCards("Province", 8);
                    break;

                case 3:
                case 4:
                    AddCards("Estate", 12);
                    AddCards("Duchy", 12);
                    AddCards("Province", 12);
                    break;
            }

            // curse cards
            switch (playerCount)
            {
                case 1:
                case 2:
                    AddCards("Curse", 10);
                    break;

                case 3:
                    AddCards("Curse", 20);
                    break;

                case 4:
                    AddCards("Curse", 30);
                    break;
            }

            // initial player deck
            AddCards("Estate", 3*playerCount);
            AddCards("Copper", 7*playerCount);
        }

        private void AddCards(string cardName, int count)
        {
            if (string.IsNullOrEmpty(cardName))
                throw new ArgumentNullException("cardName");

            var cardInfo = CardCentral.Shared.GetCardInfo(cardName);
            if (cardInfo == null)
                throw new ArgumentException("unknown card:" + cardName);

            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "Cannot be negative");

            CardPile cardPile;
            if (!cardPiles.TryGetValue(cardInfo.CardName, out cardPile))
            {
                cardPile = new CardPile();
                cardPiles.Add(cardInfo.CardName, cardPile);
            }
            cardPile.AddCards(count);
        }

        private static int CompareCardInfos(ICardInfo x, ICardInfo y)
        {
            var result = GetDisplayOrderByCardType(x) - GetDisplayOrderByCardType(y);
            if (result != 0)
                return result;

            if (x.IsTreasureCard && y.IsTreasureCard)
                return x.Coins - y.Coins;

            if (x.IsVictoryCard && y.IsVictoryCard)
                return x.VictoriyPoints - y.VictoriyPoints;

            return string.Compare(x.CardName, y.CardName, StringComparison.Ordinal);
        }

        private static int GetDisplayOrderByCardType(ICardInfo cardInfo)
        {
            if (cardInfo.IsActionCard)
                return 1;

            if (cardInfo.IsTreasureCard)
                return 2;

            if (cardInfo.IsVictoryCard)
                return 3;

            return 4;
        }
        #endregion
    }
}
