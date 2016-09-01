using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DomSample.GameObjects
{
    public class CardPile
    {
        #region properties
        public int CardCount { get; private set; }
        public bool IsEmpty
        {
            get { return (CardCount <= 0); }
        }
        #endregion

        #region methods
        public void AddCards(int count)
        {
            CardCount += count;
        }

        public int RemoveCards(int count)
        {
            if (count > CardCount)
                count = CardCount;

            CardCount -= count;

            return count;
        }
        #endregion
    }
}
