using System;
using System.Collections.Generic;
using System.Linq;

namespace DomSample.GameObjects
{
    public class Card
    {
        #region properties
        private readonly ICardInfo info; 
        #endregion

        #region constructors
        public Card(ICardInfo info)
        {
            if (info == null)
                throw new ArgumentNullException("info");
            
            this.info = info;
        }
        #endregion

        #region properties
        public ICardInfo Info
        {
            get { return info; }
        }
        #endregion

        #region helper methods
        public static Card FindCard(string cardName, IEnumerable<Card> cards)
        {
            foreach (var card in cards)
            {
                if (card.Info.CardName.Equals(cardName, StringComparison.OrdinalIgnoreCase))
                    return card;
            }
            return null;
        }

        public static Card[] FindCards(IEnumerable<string> cardNames, IEnumerable<Card> cards)
        {
            var foundCards = new HashSet<Card>();
            foreach (var cardName in cardNames)
            {
                foreach (var card in cards)
                {
                    if (!foundCards.Contains(card))
                    {
                        if (card.Info.CardName.Equals(cardName, StringComparison.OrdinalIgnoreCase))
                        {
                            foundCards.Add(card);
                            break;
                        }
                    }
                }
            }
            return foundCards.ToArray();
        }
        #endregion
    }
}
