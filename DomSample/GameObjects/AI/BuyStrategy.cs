using System.Collections.Generic;
using System.Linq;

namespace DomSample.GameObjects
{
    public class BuyStrategy
    {
        public static Instruction WeightingBuy(IGame game, AIPlayer player)
        {
            var allCards = player.GetAllPlayerCards();

            int allCardsCount = allCards.Count();
            int allActionCardcsCount = allCards.Count(card => card.Info.IsActionCard);
            int allTreasureCardcsCount = allCards.Count(card => card.Info.IsTreasureCard);

            double actionCardWeight = (double)allActionCardcsCount * 100 / allCardsCount;
            double treasureCardWeight = (double)allTreasureCardcsCount * 100 /allCardsCount;
           
            var roundAction = player.CurrentBuyStrategy;

            bool actionCardOverWeight = actionCardWeight >= roundAction.ActionCardWeighting;
            bool treasureCardOverWeight = treasureCardWeight >= roundAction.TreasureCardWeighting;
            bool actionCardMaxmized = allActionCardcsCount >= roundAction.MaxActionCards;
            bool treasureCardMaxmized = allTreasureCardcsCount >= roundAction.MaxTreasureCards;

            Instruction instruction = null;

            instruction = GeneralAIHelper.BuyVictoryCard(game, player);
            if(instruction != null)
                return instruction;

            if(!actionCardMaxmized && !treasureCardMaxmized)
            {
                if (!actionCardOverWeight && !treasureCardOverWeight)
                {
                    var actionCardSS = (roundAction.ActionCardWeighting - actionCardWeight) / roundAction.ActionCardWeighting;
                    var treasureCardSS = (roundAction.TreasureCardWeighting - treasureCardWeight) / roundAction.TreasureCardWeighting;

                    if (actionCardSS > treasureCardSS)
                        instruction = GeneralAIHelper.BuyActionCard(game, player);
                    else
                        instruction = GeneralAIHelper.BuyTresureCard(game, player);
                }
                else if (actionCardOverWeight && !treasureCardOverWeight)
                    instruction = GeneralAIHelper.BuyTresureCard(game, player);
                else if (!actionCardOverWeight && treasureCardOverWeight)
                    instruction = GeneralAIHelper.BuyActionCard(game, player);
            }
            else if(actionCardMaxmized && !treasureCardMaxmized)
            {
                if (!treasureCardOverWeight)
                    instruction = GeneralAIHelper.BuyTresureCard(game, player);
            }
            else if(!actionCardMaxmized && treasureCardMaxmized)
            {
                if(!actionCardOverWeight)
                    instruction = GeneralAIHelper.BuyActionCard(game, player);
            }

            if (instruction != null)
                return instruction;

            //buy victory cards, ignore the earilest buy vicotry card 
            return null;
        }

        // http://diehrstraits.com/2009/05/basic-dominion-strategy/
        public static Instruction BigMoneyBuy(IGame game, AIPlayer player)
        {
            int coins = player.CoinCountThisRound;

            if (coins >= 8 && GeneralAIHelper.CanBuyCard(game, "Province"))
                return GeneralAIHelper.GenerateBuyInstruction("Province");

            if (coins >= 6 && GeneralAIHelper.CanBuyCard(game, "Gold"))
                return GeneralAIHelper.GenerateBuyInstruction("Gold");

            if (coins >= 3 && GeneralAIHelper.CanBuyCard(game, "Silver"))
                return GeneralAIHelper.GenerateBuyInstruction("Silver");

            return null;
        }

        // http://diehrstraits.com/2009/05/basic-dominion-strategy/
        public static Instruction GrandSmithyBuy(IGame game, AIPlayer player)
        {
            int coins = player.CoinCountThisRound;

            if (coins >= 8 && GeneralAIHelper.CanBuyCard(game, "Province"))
                return GeneralAIHelper.GenerateBuyInstruction("Province");

            if (coins >= 6 && GeneralAIHelper.CanBuyCard(game, "Gold"))
                return GeneralAIHelper.GenerateBuyInstruction("Gold");

            if (coins >= 4)
            {
                var smithyCards = player.GetAllPlayerCards().Where(card => card.Info.CardName == "Smithy");

                if (smithyCards.Count() < 3 && GeneralAIHelper.CanBuyCard(game, "Smithy"))
                {
                    return GeneralAIHelper.GenerateBuyInstruction("Smithy");
                }
            }

            if (coins >= 3 && GeneralAIHelper.CanBuyCard(game, "Silver"))
                return GeneralAIHelper.GenerateBuyInstruction("Silver");

            return null;
        }

        //http://diehrstraits.com/2009/05/basic-dominion-strategy/
        public static Instruction ChapelBuy(IGame game, AIPlayer player)
        {
            int coins = player.CoinCountThisRound;
            if(player.ActiveCount == 1)
            {
                if(coins <= 4)
                    return GeneralAIHelper.GenerateBuyInstruction("Chapel");
            }

            if (coins >= 8 && GeneralAIHelper.CanBuyCard(game, "Province"))
                return GeneralAIHelper.GenerateBuyInstruction("Province");

            if (coins >= 6 && GeneralAIHelper.CanBuyCard(game, "Gold"))
                return GeneralAIHelper.GenerateBuyInstruction("Gold");

            if (coins >= 3 && GeneralAIHelper.CanBuyCard(game, "Silver"))
                return GeneralAIHelper.GenerateBuyInstruction("Silver");

            return null;
        }
    }
}
