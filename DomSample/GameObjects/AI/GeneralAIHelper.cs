using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DomSample.GameObjects
{
    public class GeneralAIHelper
    {
        public static Instruction PerformAI(IGame game, Player player)
        {
            // Action Stage
            if (player.CanAction())
            {
                var doActionInstruction = DoAction(player);
                if (doActionInstruction != null)
                    return doActionInstruction;
            }

            // Buy Stage
            if (player.CanBuy())
            {
                if (player is AIPlayer)
                {
                    var aiPlayer = player as AIPlayer;
                    var instruction = aiPlayer.CurrentBuyStrategy.Action.Invoke(game, aiPlayer);
                    if(instruction != null)
                        return instruction;
                }
                else
                {
                    var buyVictoryInstruction = BuyVictoryCard(game, player);
                    if (buyVictoryInstruction != null)
                        return buyVictoryInstruction;

                    //buy actions
                    var random = new Random();
                    if (random.Next(0, 10) > 5)
                    {
                        var buyActionInstruction = BuyActionCard(game, player);
                        if (buyActionInstruction != null)
                            return buyActionInstruction;
                    }
                    else
                    {
                        var buyTreasureInstruction = BuyTresureCard(game, player);
                        if (buyTreasureInstruction != null)
                            return buyTreasureInstruction;
                    }
                }
            }

            StringBuilder instructionString = new StringBuilder();
            instructionString.Append(InstructionKeyWord.Next);
            return Instruction.TryParse(instructionString.ToString());
        }

        public static Instruction BuyActionCard(IGame game, Player player)
        {
            if (!player.CanBuy())
                return null;

            var random = new Random();
            int coinCount = player.CoinCountThisRound;

            IList<ICardInfo> availableCards = new List<ICardInfo>();
            foreach (var cardInfo in game.GameInfo.ActionCards.Values)
            {
                if (coinCount >= cardInfo.Cost)
                    availableCards.Add(cardInfo);
            }

            if (availableCards.Count > 0)
            {
                ICardInfo pickedActionInfo = availableCards[random.Next(0, availableCards.Count - 1)];
                if (CanBuyCard(game, pickedActionInfo.CardName))
                    return GenerateBuyInstruction(pickedActionInfo.CardName);
            }

            return null;
        }

        public static Instruction BuyVictoryCard(IGame game, Player player)
        {
            if (!player.CanBuy())
                return null;

            int coinCount = player.CoinCountThisRound;

            if (player.PlayerType == PlayerType.AI)
            {
                if (player.ActiveCount >= ((AIPlayer)player).FirstVictoryBuyRound)
                {
                    if (coinCount >= 5)
                    {
                        if (coinCount >= 8)
                        {
                            if (CanBuyCard(game, "Province"))
                                return GenerateBuyInstruction("Province");
                        }

                        if (CanBuyCard(game, "Duchy"))
                            return GenerateBuyInstruction("Duchy");
                    }
                }
            }
            else
            {
                if (coinCount >= 8)
                {
                    if (CanBuyCard(game, "Province"))
                        return GenerateBuyInstruction("Province");
                }
            }

            return null;
        }

        public static Instruction BuyTresureCard(IGame game, Player player)
        {
            if (!player.CanBuy())
                return null;

            int coinCount = player.CoinCountThisRound;

            //buy treasures
            if (coinCount >= 6)
            {
                if (CanBuyCard(game, "Gold"))
                    return GenerateBuyInstruction("Gold");
            }
            else if (coinCount >= 3)
            {
                if (CanBuyCard(game, "Silver"))
                    return GenerateBuyInstruction("Silver");
            }

            var moneyLenderCards = player.GetAllPlayerCards().Where(card => card.Info.CardName == "MoneyLender");
            if (moneyLenderCards.Count() > 0)
                return GenerateBuyInstruction("Copper");

            return null;
        }

        public static Instruction GenerateBuyInstruction(string cardName)
        {
            StringBuilder instructionString = new StringBuilder();
            instructionString.Append(InstructionKeyWord.Buy).Append(" ").Append(cardName);
            return Instruction.TryParse(instructionString.ToString());
        }

        public static Boolean CanBuyCard(IGame game, string cardName)
        {
            if (!game.CardPiles.ContainsKey(cardName))
                return false;

            if (game.CardPiles[cardName].IsEmpty)
                return false;

            return true;
        }

        public static Instruction DoAction(Player player)
        {
            // Action stage
            if (player.CanAction())
            {
                var random = new Random();
                StringBuilder instructionString = new StringBuilder();
                IList<Card> actionCardsInhand = new List<Card>();
                foreach (var card in player.HandCards)
                {
                    if (card.Info.PlusActions > 0)
                    {
                        instructionString.Append(InstructionKeyWord.Play).Append(" ").Append(card.Info.CardName);
                        return Instruction.TryParse(instructionString.ToString());
                    }

                    if (card.Info.IsActionCard && !(card.Info.CardName == "Remodel" && player.ActiveCount <= 6))
                        actionCardsInhand.Add(card);
                }

                if (actionCardsInhand.Count > 0)
                {
                    Card pickedActionInfo = actionCardsInhand[random.Next(0, actionCardsInhand.Count - 1)];
                    instructionString.Append(InstructionKeyWord.Play).Append(" ").Append(pickedActionInfo.Info.CardName);
                    return Instruction.TryParse(instructionString.ToString());
                }
            }

            return null;
        }
    }
}
