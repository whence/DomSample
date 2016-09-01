using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DomSample.GameObjects
{
    public class BigMoneyAIPlayer : AIPlayer
    {
        public BigMoneyAIPlayer(String playerName, IEnumerable<Card> initialCards) : base(playerName, initialCards)
        {
            this.AddBuyStageStrategy(0, BuyStrategy.BigMoneyBuy);
        }
    }
}
