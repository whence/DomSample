using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DomSample.GameObjects
{
    public class GameInfo : IGameInfo
    {
        #region fields
        private readonly Dictionary<string, ICardInfo> kingdomCards;
        private readonly Dictionary<string, ICardInfo> actionCards;
        private readonly Dictionary<string, ICardInfo> attackCards;
        private readonly Dictionary<string, ICardInfo> reactionCards;
        private readonly Dictionary<string, ICardInfo> victoryCards;
        private readonly Dictionary<string, ICardInfo> treasureCards;
        #endregion

        #region IGameInfo
        public IDictionary<String, ICardInfo> KingdomCards
        {
            get { return kingdomCards; }
        }

        public IDictionary<String, ICardInfo> ActionCards
        {
            get { return actionCards; }
        }

        public IDictionary<String, ICardInfo> AttackCards
        {
            get { return attackCards; }
        }

        public IDictionary<String, ICardInfo> ReactionCards
        {
            get { return reactionCards; }
        }

        public IDictionary<String, ICardInfo> VictoryCards
        {
            get { return victoryCards; }
        }

        public IDictionary<String, ICardInfo> TreasureCards
        {
            get { return treasureCards; }
        }
        #endregion

        public GameInfo(ICollection<string> kingdomCardNames)
        {
            kingdomCards = new Dictionary<string, ICardInfo>(kingdomCardNames.Count);
            actionCards = new Dictionary<string, ICardInfo>();
            attackCards = new Dictionary<string, ICardInfo>();
            reactionCards = new Dictionary<string, ICardInfo>();
            victoryCards = new Dictionary<string, ICardInfo>();
            treasureCards = new Dictionary<string, ICardInfo>();

            foreach (var cardName in kingdomCardNames)
            {
                ICardInfo cardInfo = CardCentral.Shared.GetCardInfo(cardName);
                   
                if(cardInfo.IsVictoryCard)
                {
                    victoryCards.Add(cardName, cardInfo);
                }
                else
                {
                    if (!kingdomCards.ContainsKey(cardName))
                    {
                        kingdomCards.Add(cardName, cardInfo);
                        if (cardInfo.IsActionCard) actionCards.Add(cardName, cardInfo);
                        if (cardInfo.IsAttackCard) attackCards.Add(cardName, cardInfo);
                        if (cardInfo.IsDefendCard) reactionCards.Add(cardName, cardInfo);
                    }
                }
            }

            treasureCards .Add("Copper", CardCentral.Shared.GetCardInfo("Copper"));
            treasureCards.Add("Silver", CardCentral.Shared.GetCardInfo("Silver"));
            treasureCards.Add("Gold", CardCentral.Shared.GetCardInfo("Gold"));

            victoryCards.Add("Estate", CardCentral.Shared.GetCardInfo("Estate"));
            victoryCards.Add("Duchy", CardCentral.Shared.GetCardInfo("Duchy"));
            victoryCards.Add("Province", CardCentral.Shared.GetCardInfo("Province"));
            victoryCards.Add("Curse", CardCentral.Shared.GetCardInfo("Curse"));
        }
    }
}
