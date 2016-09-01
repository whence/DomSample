using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DomSample.GameObjects
{
    public class WeightingBuyAIPlayer : AIPlayer
    {
        public WeightingBuyAIPlayer(String playerName, IEnumerable<Card> initialCards) : base(playerName, initialCards)
        {
            this.AddBuyStageStrategy(new RoundAction(0, BuyStrategy.WeightingBuy)
            {
                ActionCardWeighting = 10,
                TreasureCardWeighting = 80,
                MaxTreasureCards = 15,
                MaxActionCards = 1,
            });

            this.FirstVictoryBuyRound = 12;
        }
    }
}
