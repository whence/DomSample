using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomSample.Utils;

namespace DomSample.GameObjects
{
    public class SpecialActionAI
    {
        // Normal level
        public static string ResolveCardCellarAI(Player player)
        {
            StringBuilder discardCards = new StringBuilder();

            IEnumerable<Card> victoryCards = player.HandCards.Where(card => card.Info.IsVictoryCard);
            IEnumerable<Card> copperCards = player.HandCards.Where(card => card.Info.CardName == "Copper");

            IEnumerable<Card> allCards = player.GetAllPlayerCards();
            
            foreach (var card in victoryCards)
            {
                discardCards.Append(card.Info.CardName).Append(",");
            }

            if(copperCards.Count() > 0)
            {
                int replaceNumber = 0;
                double replaceRate = 0;

                IEnumerable<Card> betterCards = from card in allCards
                                                where (card.Info.IsActionCard &&
                                                      (card.Info.PlusActions > 0 || card.Info.PlusCoins > 1)) 
                                                      || (card.Info.Coins > 1)
                                                select card;

                for(int i = 1; i<=copperCards.Count();i++)
                {
                    var betterCombination = Maths.Choose(betterCards.Count(), i);
                    var totalCombination = Maths.Choose(allCards.Count(), i);
                    double rate = (double)betterCombination/(double)totalCombination;
                    if(rate > replaceRate)
                    {
                        replaceNumber = i;
                        replaceRate = rate;
                    }
                }

                if(replaceRate >= 0.2)
                {
                    for (int i = 0; i < replaceNumber; i++)
                    {
                        discardCards.Append("Copper,");
                    }
                }
            }
            if (discardCards.Length > 0)
                discardCards.Remove(discardCards.Length - 1, 1);
            return discardCards.ToString();
        }

        internal static string ResolveCardMilitiaAI(Player player)
        {
            StringBuilder discardCards = new StringBuilder();

            int noOfCardsToDiscard = player.HandCards.Count() - 3;

            IEnumerable<Card> victoryCards = player.HandCards.Where(card => card.Info.IsVictoryCard);
            if(noOfCardsToDiscard >= victoryCards.Count())
            {
                foreach (var victoryCard in victoryCards)
                {
                    discardCards.Append(victoryCard.Info.CardName).Append(",");
                }
                
                noOfCardsToDiscard = noOfCardsToDiscard - victoryCards.Count();
                IEnumerable<Card> copperCards = player.HandCards.Where(card => card.Info.CardName == "Copper");
                
                if(noOfCardsToDiscard >= copperCards.Count())
                {
                    for (int i = 0; i < copperCards.Count(); i++)
                    {
                        discardCards.Append("Copper,");
                    }
                    noOfCardsToDiscard = noOfCardsToDiscard - copperCards.Count();

                    IEnumerable<Card> actionCards = player.HandCards.Where(card => card.Info.IsActionCard).OrderByDescending(card => card.Info.Cost);
                    
                    if(noOfCardsToDiscard >= actionCards.Count())
                    {
                        foreach (var actionCard in actionCards)
                        {
                            discardCards.Append(actionCard.Info.CardName).Append(",");
                        }
                        noOfCardsToDiscard = noOfCardsToDiscard - actionCards.Count();

                        IEnumerable<Card> silverGoldCards = player.HandCards.Where(card => card.Info.IsTreasureCard && card.Info.Cost > 0);
                        for (int i = 0; i < noOfCardsToDiscard; i++)
                        {
                            discardCards.Append(EnumerableHelper.ElementAt(silverGoldCards, i).Info.CardName).Append(",");
                        }
                    }
                    else
                    {
                        for (int i = 0; i < noOfCardsToDiscard; i++)
                        {
                            discardCards.Append(EnumerableHelper.ElementAt(actionCards, i).Info.CardName).Append(",");
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < noOfCardsToDiscard; i++)
                    {
                        discardCards.Append("Copper,");
                    }
                }
            }
            else
            {
                for (int i = 0; i < noOfCardsToDiscard; i++)
                {
                    discardCards.Append(EnumerableHelper.ElementAt(victoryCards, i).Info.CardName).Append(",");
                }
            }
            if (discardCards.Length > 0)
                discardCards.Remove(discardCards.Length - 1, 1);
            return discardCards.ToString();
        }

        // Normal level
        internal static string ResolveCardRemodelTrashCardAI(Player player)
        {
            if (player.HandCards.Count() > 0)
            {
                var curseCard = player.HandCards.FirstOrDefault(card => card.Info.VictoriyPoints < 0);
                if(curseCard != null)
                    return curseCard.Info.CardName;

                var cost4Card = player.HandCards.FirstOrDefault(card => card.Info.Cost == 4);
                if (cost4Card != null)
                    return cost4Card.Info.CardName;

                if(player is AIPlayer && player.ActiveCount > ((AIPlayer)player).FirstVictoryBuyRound)
                {
                    var cost6Card = player.HandCards.FirstOrDefault(card => card.Info.Cost == 6);
                    if(cost6Card != null)
                        return cost6Card.Info.CardName;
                }

                var copperCard = player.HandCards.FirstOrDefault(card => card.Info.CardName == "Copper");
                if (copperCard != null)
                    return "Copper";

                int minCost = player.HandCards.Min(card => card.Info.Cost);
                var minCostCards = player.HandCards.Where(card => card.Info.Cost == minCost);
                var minCostActionCards = minCostCards.Where(card => card.Info.IsActionCard);
                
                if(minCostActionCards.Count() > 0)
                    return EnumerableHelper.ElementAt(minCostActionCards, 0).Info.CardName;
                else
                    return EnumerableHelper.ElementAt(minCostCards, 0).Info.CardName;
            }
           
            return string.Empty;
        }

        // Normal level
        internal static string ResolveCardRemodelGainCardAI(IGame game, Player player, Card cardToTrash)
        {
            int coins = cardToTrash.Info.Cost + 2;
            if (coins == 8)
                return "Province";

            // if gold not existed, not doing the remodel
            if (coins == 6)
            {
                // TODO: need to check card existed
                if (game.CardPiles["Gold"].IsEmpty)
                    return cardToTrash.Info.CardName;
                return "Gold";
            }

            if(coins == 5)
            {
                if(player is AIPlayer && (player as AIPlayer).FirstVictoryBuyRound >= player.ActiveCount)
                {
                    if(GeneralAIHelper.CanBuyCard(game, "Duchy")) 
                        return "Duchy";
                }
            }

            var availalbeCards = game.GameInfo.ActionCards.Where(c => c.Value.Cost == coins).Select(c => c.Value);
            if(availalbeCards.Count() >0)
            {
                var canBuyCards = availalbeCards.Where(cardInfo => GeneralAIHelper.CanBuyCard(game, cardInfo.CardName));
                if(canBuyCards.Count() > 0)
                {
                    var random = new Random();

                    return EnumerableHelper.ElementAt(canBuyCards, random.Next(0, canBuyCards.Count() - 1)).CardName;
                }
            }

            availalbeCards = game.GameInfo.ActionCards.Where(c => c.Value.Cost == coins-1).Select(c => c.Value);
            if (availalbeCards.Count() > 0)
            {
                var canBuyCards = availalbeCards.Where(cardInfo => GeneralAIHelper.CanBuyCard(game, cardInfo.CardName));
                if (canBuyCards.Count() > 0)
                {
                    var random = new Random();

                    return EnumerableHelper.ElementAt(canBuyCards, random.Next(0, canBuyCards.Count() - 1)).CardName;
                }
            }

            //TODO: need to check card available
            return cardToTrash.Info.CardName;
        }
    }
}