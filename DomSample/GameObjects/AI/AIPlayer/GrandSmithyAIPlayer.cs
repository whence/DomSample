using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DomSample.GameObjects
{
    public class GrandSmithyAIPlayer : AIPlayer
    {
        public GrandSmithyAIPlayer(string playerName, IEnumerable<Card> initialCards) : base(playerName, initialCards)
        {
            this.AddBuyStageStrategy(0, BuyStrategy.GrandSmithyBuy);
        }
    }
}
