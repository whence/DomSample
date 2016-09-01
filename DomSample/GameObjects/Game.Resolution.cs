using System;
using System.Collections.Generic;
using DomSample.Utils;

namespace DomSample.GameObjects
{
    public partial class Game
    {
        private Dictionary<string, Action<Player, Card>> CreateCardResolvers()
        {
            var resolvers = new Dictionary<string, Action<Player, Card>>(40, StringComparer.OrdinalIgnoreCase);

            resolvers.Add("Adventurer", ResolveCardAdventurer);
            resolvers.Add("Bureaucrat", ResolveCardBureaucrat);
            resolvers.Add("Cellar", ResolveCardCellar);
            resolvers.Add("Chancellor", ResolveCardChancellor);
            resolvers.Add("Chapel", ResolveCardChapel);
            resolvers.Add("Council Room", ResolveCardCouncilRoom);
            resolvers.Add("Feast", ResolveCardFeast);
            resolvers.Add("Festival", ResolveCardFestival);
            resolvers.Add("Laboratory", ResolveCardLaboratory);
            resolvers.Add("Library", ResolveCardLibrary);
            resolvers.Add("Market", ResolveCardMarket);
            resolvers.Add("Militia", ResolveCardMilitia);
            resolvers.Add("Mine", ResolveCardMine);
            resolvers.Add("Moat", ResolveCardMoat);
            resolvers.Add("Moneylender", ResolveCardMoneylender);
            resolvers.Add("Remodel", ResolveCardRemodel);
            resolvers.Add("Smithy", ResolveCardSmithy);
            resolvers.Add("Spy", ResolveCardSpy);
            resolvers.Add("Thief", ResolveCardThief);
            resolvers.Add("Throne Room", ResolveCardThroneRoom);
            resolvers.Add("Village", ResolveCardVillage);
            resolvers.Add("Witch", ResolveCardWitch);
            resolvers.Add("Woodcutter", ResolveCardWoodcutter);
            resolvers.Add("Workshop", ResolveCardWorkshop);

            return resolvers;
        }

        // Reveal cards from your deck until you reveal 2 Treasure cards. 
        // Put those cards into your hand and discard the other revealed cards (late stage) 
        private void ResolveCardAdventurer(Player activePlayer, Card cardToResolve)
        {
            var treasureCards = new List<Card>(2);
            var otherRevealedCards = new List<Card>();
            while (treasureCards.Count < 2)
            {
                var revealedCard = activePlayer.RevealTopCardFromDeck();
                if (revealedCard == null)
                    break;

                if (revealedCard.Info.IsTreasureCard)
                {
                    treasureCards.Add(revealedCard);
                }
                else
                {
                    otherRevealedCards.Add(revealedCard);
                }
            }

            activePlayer.DisplayRevealCards(writer);

            activePlayer.UnrevealCardsToHand(treasureCards.ToArray());
            activePlayer.DiscardRevealCards(otherRevealedCards.ToArray());
        }

        // Gain a silver. Put it on top of your deck
        // Each other player reveals a Victory card from his hand and puts it on his deck  
        // (or reveals a hand with no Victory cards) 
        private void ResolveCardBureaucrat(Player activePlayer, Card cardToResolve)
        {
            var cardToGain = supply.GetCard(CardCentral.Shared.GetCardInfo("Silver"));
            if (cardToGain != null)
            {
                activePlayer.GainCardOnDeck(cardToGain);
            }

            using (var loop = new LoopEnumerator<Player>(players, activePlayerIndex, 1))
            {
                loop.Reset();
                while (loop.MoveNext())
                {
                    var player = loop.Current;
                    if (player != activePlayer && !TryDefendAttack(player))
                    {
                        player.DisplayInfo(writer);
                        
                        Card cardToReveal;
                        do
                        {
                            writer.Write("{0}, select a victory card to reveal (empty to skip and reveal the whole hand):", player.Name);
                            var cardNameToReveal = Texts.Trim(reader.ReadLine());
                            if (string.IsNullOrEmpty(cardNameToReveal))
                            {
                                cardToReveal = null;

                                if (!player.HasVictoryCardOnHand())
                                    break;
                                
                                writer.Write("You cannot skip as you have victory cards in hand");
                            }
                            else
                            {
                                cardToReveal = player.FindCardOnHand(cardNameToReveal);
                                if (cardToReveal == null)
                                {
                                    writer.WriteLine("{0} is not in your hand", cardNameToReveal);
                                }
                                else if (!cardToReveal.Info.IsVictoryCard)
                                {
                                    writer.WriteLine("{0} is not a victory card", cardToReveal.Info.CardName);
                                    cardToReveal = null;
                                }
                            }
                        } while (cardToReveal == null);

                        if (cardToReveal != null)
                        {
                            player.RevealHandCard(cardToReveal);
                            player.DisplayRevealCards(writer);
                            player.UnrevealCardToTopDeck(cardToReveal);
                        }
                        else
                        {
                            var handCards = player.RevealAllHandCards();
                            player.DisplayRevealCards(writer);
                            player.UnrevealCardsToHand(handCards);
                        }
                    }
                }
            }
        }

        // +1 Action
        // Discard any number of cards. +1 Card per card discard   
        private void ResolveCardCellar(Player activePlayer, Card cardToResolve)
        {
            activePlayer.AddAction(1);

            writer.Write("cards to discard:");
            String discardCards;
            if (activePlayer is AIPlayer)
            {
                discardCards = SpecialActionAI.ResolveCardCellarAI(activePlayer);
                writer.Write(discardCards);
                writer.WriteLine();
            }
            else
                discardCards = reader.ReadLine();
            var cardsToDiscard = activePlayer.FindCardsOnHand(Texts.SplitCsv(discardCards));
            
            activePlayer.DiscardHandCards(cardsToDiscard);
            activePlayer.DrawCards(cardsToDiscard.Length);
        }

        // +2 Coins
        // Immediately put your deck into your discard pile (based on last cards in deck) 
        private void ResolveCardChancellor(Player activePlayer, Card cardToResolve)
        {
            activePlayer.AddCoinBonus(2);

            writer.Write("Would you like to discard your deck?");
            if (Texts.ParseBoolean(reader.ReadLine()) ?? true)
            {
                activePlayer.DiscardDeckTopDown();
            }
        }

        // Trash up to 4 cards from your hand
        private void ResolveCardChapel(Player activePlayer, Card cardToResolve)
        {
            Card[] cardsToTrash;
            do
            {
                writer.Write("select up to 4 cards to trash (empty to skip):");
                cardsToTrash = activePlayer.FindCardsOnHand(Texts.SplitCsv(reader.ReadLine()));

                if (cardsToTrash.Length > 4)
                {
                    writer.WriteLine("too many cards to trash");
                    cardsToTrash = null;
                }
            } while (cardsToTrash == null);

            if (cardsToTrash.Length > 0)
            {
                activePlayer.TrashHandCards(cardsToTrash, trashCards);
            }
        }

        // +1 Buy 
        // +4 Cards
        // Each other player draws a card 
        private void ResolveCardCouncilRoom(Player activePlayer, Card cardToResolve)
        {
            activePlayer.DrawCards(4);
            activePlayer.AddBuy(1);

            using (var loop = new LoopEnumerator<Player>(players, activePlayerIndex, 1))
            {
                loop.Reset();
                while (loop.MoveNext())
                {
                    var player = loop.Current;
                    if (player != activePlayer)
                    {
                        player.DrawCards(1);
                    }
                }
            }
        }

        // Trash this card. Gain a card costing up to Coin 5 
        private void ResolveCardFeast(Player activePlayer, Card cardToResolve)
        {
            if (activePlayer.IsCardInPlay(cardToResolve)) // this check is necessary as player can use throne room to play feast twice
            {
                activePlayer.TrashPlayedCard(cardToResolve, trashCards);
            }

            Card cardToGain;
            do
            {
                writer.Write("select a card to gain (up to 5 coins):");
                var cardNameToGain = Texts.Trim(reader.ReadLine());
                var cardInfoToGain = CardCentral.Shared.GetCardInfo(cardNameToGain);
                if (cardInfoToGain == null)
                {
                    writer.WriteLine("unknown card " + cardNameToGain);
                    cardToGain = null;
                }
                else if (cardInfoToGain.Cost > 5)
                {
                    writer.WriteLine("{0} is too expensive to gain", cardInfoToGain.CardName);
                    cardToGain = null;
                }
                else
                {
                    cardToGain = supply.GetCard(cardInfoToGain);
                    if (cardToGain == null)
                    {
                        writer.WriteLine("{0} is not in supply", cardInfoToGain.CardName);
                    }
                }
            } while (cardToGain == null);

            activePlayer.GainCard(cardToGain);
        }

        // +2 Actions
        // +1 Buy
        // +2 Coins 
        private static void ResolveCardFestival(Player activePlayer, Card cardToResolve)
        {
            activePlayer.AddAction(2);
            activePlayer.AddBuy(1);
            activePlayer.AddCoinBonus(2);
        }

        // +1 Action
        // +2 Cards 
        private static void ResolveCardLaboratory(Player activePlayer, Card cardToResolve)
        {
            activePlayer.DrawCards(2);
            activePlayer.AddAction(1);
        }

        // Draw until you have 7 cards in hand. 
        // You may set aside any Action cards drawn this way, as you draw them; 
        // discard the set aside cards after you finish drawing. 
        private void ResolveCardLibrary(Player activePlayer, Card cardToResolve)
        {
            while (activePlayer.HandCardsCount < 7)
            {
                var drawnCard = activePlayer.DrawOneCard();
                if (drawnCard == null)
                    break;

                if (drawnCard.Info.IsActionCard)
                {
                    writer.Write("You've drawn an action card:{0}, would you like to set aside it?", drawnCard.Info.CardName);
                    if (Texts.ParseBoolean(reader.ReadLine()) ?? false)
                    {
                        activePlayer.RevealHandCard(drawnCard);
                    }
                }
            }

            activePlayer.DisplayRevealCards(writer);
            activePlayer.DiscardAllRevealCards();
        }

        // +1 Action
        // +1 Card
        // +1 Buy
        // +1 Coin 
        private static void ResolveCardMarket(Player activePlayer, Card cardToResolve)
        {
            activePlayer.DrawCards(1);
            activePlayer.AddAction(1);
            activePlayer.AddBuy(1);
            activePlayer.AddCoinBonus(1);
        }

        // +2 Coins
        // Each other player discards down to 3 cards in his hand. 
        private void ResolveCardMilitia(Player activePlayer, Card cardToResolve)
        {
            activePlayer.AddCoinBonus(2);

            using (var loop = new LoopEnumerator<Player>(players, activePlayerIndex, 1))
            {
                loop.Reset();
                while (loop.MoveNext())
                {
                    var player = loop.Current;
                    if (player != activePlayer && !TryDefendAttack(player))
                    {
                        player.DisplayInfo(writer);
                        while (player.HandCardsCount > 3)
                        {
                            writer.Write("{0}, choose {1} cards to discard:", player.Name, player.HandCardsCount - 3);
                            String discardCards;
                            if (player is AIPlayer)
                            {
                                discardCards = SpecialActionAI.ResolveCardMilitiaAI(player);
                                writer.Write(discardCards);
                                writer.WriteLine();
                            }
                            else
                                discardCards = reader.ReadLine();
                            var cardsToDiscard = player.FindCardsOnHand(Texts.SplitCsv(discardCards));
                            if (cardsToDiscard.Length == player.HandCardsCount - 3)
                            {
                                player.DiscardHandCards(cardsToDiscard);
                            }
                            else
                            {
                                writer.WriteLine("You must choose exactly {0} cards to discard", player.HandCardsCount - 3);
                            }
                        }
                    }
                }
            }
        }

        // Trash a Treasure card from your hand.
        // Gain a Treausre card costing +3 Coins. put it into your hand.  
        private void ResolveCardMine(Player activePlayer, Card cardToResolve)
        {
            Card cardToTrash;
            do
            {
                writer.Write("select a treasury card to trash (empty to skip):");
                var cardNameToTrash = Texts.Trim(reader.ReadLine());
                if (string.IsNullOrEmpty(cardNameToTrash))
                {
                    cardToTrash = null;

                    if (!activePlayer.HasTreasureCardOnHand())
                        break;

                    writer.WriteLine("You cannot skip as you have treasure cards in hand");
                }
                else
                {
                    cardToTrash = activePlayer.FindCardOnHand(cardNameToTrash);
                    if (cardToTrash == null)
                    {
                        writer.WriteLine("{0} is not in your hand", cardNameToTrash);
                    }
                    else if (!cardToTrash.Info.IsTreasureCard)
                    {
                        writer.WriteLine("{0} is not a treasury card", cardToTrash.Info.CardName);
                        cardToTrash = null;
                    }
                }
            } while (cardToTrash == null);

            if (cardToTrash != null)
            {
                activePlayer.TrashHandCard(cardToTrash, trashCards);

                Card cardToGain;
                do
                {
                    writer.Write("select a treasury card to gain (up to {0} coins):", cardToTrash.Info.Cost + 3);
                    var cardNameToGain = Texts.Trim(reader.ReadLine());
                    var cardInfoToGain = CardCentral.Shared.GetCardInfo(cardNameToGain);
                    if (cardInfoToGain == null)
                    {
                        writer.WriteLine("unknown card " + cardNameToGain);
                        cardToGain = null;
                    }
                    else if (!cardInfoToGain.IsTreasureCard)
                    {
                        writer.WriteLine("{0} is not a treasury card", cardInfoToGain.CardName);
                        cardToGain = null;
                    }
                    else if (cardInfoToGain.Cost > cardToTrash.Info.Cost + 3)
                    {
                        writer.WriteLine("{0} is too expensive to gain", cardInfoToGain.CardName);
                        cardToGain = null;
                    }
                    else
                    {
                        cardToGain = supply.GetCard(cardInfoToGain);
                        if (cardToGain == null)
                        {
                            writer.WriteLine("{0} is not in supply", cardInfoToGain.CardName);
                        }
                    }
                } while (cardToGain == null);

                activePlayer.GainCardOnHand(cardToGain);
            }
        }

        // + 2 Cards
        // When another player plays an Attack card, you may reveal this from your hand. 
        // If you do, you are unaffected by that Attack.  
        private static void ResolveCardMoat(Player activePlayer, Card cardToResolve)
        {
            activePlayer.DrawCards(2);
        }

        // Trash a Copper card from your hand. If you do, +3 Coins 
        private void ResolveCardMoneylender(Player activePlayer, Card cardToResolve)
        {
            var copperCard = activePlayer.FindCardOnHand("copper");
            if (copperCard != null)
            {
                activePlayer.TrashHandCard(copperCard, trashCards);
                activePlayer.AddCoinBonus(3);
            }
            else
            {
                writer.WriteLine("You don't have a Copper to trash");
            }
        }

        // Trash a card from your hand. Gain a card costing up to +2 coins more than the trashed card. 
        private void ResolveCardRemodel(Player activePlayer, Card cardToResolve)
        {
            Card cardToTrash;
            do
            {
                writer.Write("select a card to trash (empty to skip):");
                string trashCard;
                if (activePlayer is AIPlayer)
                {
                    trashCard = SpecialActionAI.ResolveCardRemodelTrashCardAI(activePlayer);
                    writer.Write(trashCard);
                    writer.WriteLine();
                }
                else
                    trashCard = reader.ReadLine();
                var cardNameToTrash = Texts.Trim(trashCard);
                if (string.IsNullOrEmpty(cardNameToTrash))
                {
                    cardToTrash = null;

                    if (activePlayer.HandCardsCount == 0)
                        break;

                    writer.Write("You cannot skip as you have cards in hand");
                }
                else
                {
                    cardToTrash = activePlayer.FindCardOnHand(cardNameToTrash);
                    if (cardToTrash == null)
                    {
                        writer.WriteLine("{0} is not in your hand", cardNameToTrash);
                    }
                }
            } while (cardToTrash == null);

            if (cardToTrash != null)
            {
                activePlayer.TrashHandCard(cardToTrash, trashCards);

                Card cardToGain;
                do
                {
                    writer.Write("select a card to gain (up to {0} coins):", cardToTrash.Info.Cost + 2);
                    string gainCard;
                    if (activePlayer is AIPlayer)
                    {
                        gainCard = SpecialActionAI.ResolveCardRemodelGainCardAI(this, activePlayer, cardToTrash);
                        writer.Write(gainCard);
                        writer.WriteLine();
                    }
                    else
                        gainCard = reader.ReadLine();

                    var cardNameToGain = Texts.Trim(gainCard);
                    var cardInfoToGain = CardCentral.Shared.GetCardInfo(cardNameToGain);
                    if (cardInfoToGain == null)
                    {
                        writer.WriteLine("unknown card " + cardNameToGain);
                        cardToGain = null;
                    }
                    else if (cardInfoToGain.Cost > cardToTrash.Info.Cost + 2)
                    {
                        writer.WriteLine("{0} is too expensive to gain", cardInfoToGain.CardName);
                        cardToGain = null;
                    }
                    else
                    {
                        cardToGain = supply.GetCard(cardInfoToGain);
                        if (cardToGain == null)
                        {
                            writer.WriteLine("{0} is not in supply", cardInfoToGain.CardName);
                        }
                    }
                } while (cardToGain == null);

                activePlayer.GainCard(cardToGain);
            }
        }

        // +3 Cards 
        private static void ResolveCardSmithy(Player activePlayer, Card cardToResolve)
        {
            activePlayer.DrawCards(3);
        }

        // +1 Action
        // +1 Card
        // Each player (including you) reveals the top card of his deck and either discards it or puts it back, 
        // your choice. 
        private void ResolveCardSpy(Player activePlayer, Card cardToResolve)
        {
            activePlayer.DrawCards(1);
            activePlayer.AddAction(1);

            using (var loop = new LoopEnumerator<Player>(players, activePlayerIndex, 1))
            {
                loop.Reset();
                while (loop.MoveNext())
                {
                    var player = loop.Current;

                    if (player == activePlayer || !TryDefendAttack(player))
                    {
                        var revealedCard = player.RevealTopCardFromDeck();
                        if (revealedCard != null)
                        {
                            writer.WriteLine("{0} revealed {1}", player.Name, revealedCard.Info.CardName);
                            writer.Write("Would you like to discard it?");
                            if (Texts.ParseBoolean(reader.ReadLine()) ?? true)
                            {
                                player.DiscardRevealCard(revealedCard);
                            }
                            else
                            {
                                player.UnrevealCardToTopDeck(revealedCard);
                            }
                        }
                    }
                }
            }
        }

        // Each other player reveals the top 2 cards of his deck. 
        // If they revealed any Treasure cards, they trash one of them that you choose. 
        // You may gain any or all of these trashed cards. They discard the other revealed cards. 
        private void ResolveCardThief(Player activePlayer, Card cardToResolve)
        {
            var tempTrashCards = new List<Card>();

            using (var loop = new LoopEnumerator<Player>(players, activePlayerIndex, 1))
            {
                loop.Reset();
                while (loop.MoveNext())
                {
                    var player = loop.Current;
                    if (player != activePlayer && !TryDefendAttack(player))
                    {
                        var treasureCards = new List<Card>();
                        var revealedCards = new List<Card>(2);
                        while (revealedCards.Count < 2)
                        {
                            var revealedCard = player.RevealTopCardFromDeck();
                            if (revealedCard == null)
                                break;

                            revealedCards.Add(revealedCard);

                            if (revealedCard.Info.IsTreasureCard)
                            {
                                treasureCards.Add(revealedCard);
                            }
                        }

                        if (treasureCards.Count > 0)
                        {
                            writer.Write("{0} has revealed {1} treasure cards:", player.Name, treasureCards.Count);
                            foreach (var card in treasureCards)
                            {
                                writer.Write(card.Info.CardName);
                                writer.Write(", ");
                            }
                            writer.WriteLine();

                            Card cardToTrash;
                            do
                            {
                                writer.Write("Choose one to trash:");
                                var cardNameToTrash = Texts.Trim(reader.ReadLine());
                                cardToTrash = Card.FindCard(cardNameToTrash, treasureCards);

                                if (cardToTrash == null)
                                {
                                    writer.WriteLine("{0} is not in the options", cardNameToTrash);
                                }
                            } while (cardToTrash == null);

                            player.TrashRevealCard(cardToTrash, tempTrashCards);
                            revealedCards.Remove(cardToTrash);
                        }

                        player.DiscardRevealCards(revealedCards.ToArray());
                    }
                }
            }

            if (tempTrashCards.Count > 0)
            {
                writer.Write("Trash cards to gain:");
                foreach (var card in tempTrashCards)
                {
                    writer.Write(card.Info.CardName);
                    writer.Write(", ");
                }
                writer.WriteLine();

                writer.Write("Choose which to gain (type all to gain all):");
                var cardNamesToGain = Texts.SplitCsv(reader.ReadLine());
                Card[] cardsToGain;
                if (cardNamesToGain.Length == 1 && "all".Equals(cardNamesToGain[0], StringComparison.OrdinalIgnoreCase))
                {
                    cardsToGain = tempTrashCards.ToArray();
                }
                else
                {
                    cardsToGain = Card.FindCards(cardNamesToGain, tempTrashCards);
                }

                activePlayer.GainCards(cardsToGain);
                tempTrashCards.RemoveRange(cardsToGain);
            }

            trashCards.AddRange(tempTrashCards);
        }

        // Choose one Action card in your hand. Play it twice. 
        private void ResolveCardThroneRoom(Player activePlayer, Card cardToResolve)
        {
            if (activePlayer.HasActionCardOnHand())
            {
                Card cardToPlay;
                do
                {
                    writer.Write("select an action card to play twice: (empty to skip)");
                    var cardNameToPlay = Texts.Trim(reader.ReadLine());
                    if (string.IsNullOrEmpty(cardNameToPlay))
                    {
                        cardToPlay = null;

                        if (!activePlayer.HasActionCardOnHand())
                            break;

                        writer.Write("You cannot skip as you have action cards in hand");
                        activePlayer.DisplayInfo(writer);
                    }
                    else
                    {
                        cardToPlay = activePlayer.FindCardOnHand(cardNameToPlay);
                        if (cardToPlay == null)
                        {
                            writer.WriteLine("{0} is not in your hand", cardNameToPlay);
                        }
                        else if (!cardToPlay.Info.IsActionCard)
                        {
                            writer.WriteLine("{0} is not an action card", cardToPlay.Info.CardName);
                            cardToPlay = null;
                        }
                    }
                } while (cardToPlay == null);

                if (cardToPlay != null)
                {
                    activePlayer.PlayCard(cardToPlay);

                    Action<Player, Card> cardResolver;
                    if (!cardResolvers.TryGetValue(cardToPlay.Info.CardName, out cardResolver))
                        throw new Exception(string.Format("Action card {0} has no resolver", cardToPlay.Info.CardName));

                    for (int i = 0; i < 2; i++)
                    {
                        cardResolver(activePlayer, cardToPlay);
                    }
                }
            }
            else
            {
                writer.WriteLine("You played {0}, but you don't have action card to throne", cardToResolve.Info.CardName);
            }
        }

        // +2 Actions
        // +1 Card 
        private static void ResolveCardVillage(Player activePlayer, Card cardToResolve)
        {
            activePlayer.DrawCards(1);
            activePlayer.AddAction(2);
        }

        // +2 Cards
        // Each other player gains a Curse  
        private void ResolveCardWitch(Player activePlayer, Card cardToResolve)
        {
            activePlayer.DrawCards(2);

            using (var loop = new LoopEnumerator<Player>(players, activePlayerIndex, 1))
            {
                loop.Reset();
                while (loop.MoveNext())
                {
                    var player = loop.Current;
                    if (player != activePlayer && !TryDefendAttack(player))
                    {
                        var cardToGain = supply.GetCard(CardCentral.Shared.GetCardInfo("Curse"));
                        if (cardToGain != null)
                        {
                            player.GainCard(cardToGain);
                        }
                    }
                }
            }
        }

        // +2 Coins
        // +1 Buy 
        private static void ResolveCardWoodcutter(Player activePlayer, Card cardToResolve)
        {
            activePlayer.AddBuy(1);
            activePlayer.AddCoinBonus(2);
        }

        // Gain a card costing up to (4) 
        private void ResolveCardWorkshop(Player activePlayer, Card cardToResolve)
        {
            Card cardToGain;
            do
            {
                writer.Write("select a card to gain (up to 4 coins):");
                var cardNameToGain = Texts.Trim(reader.ReadLine());
                var cardInfoToGain = CardCentral.Shared.GetCardInfo(cardNameToGain);
                if (cardInfoToGain == null)
                {
                    writer.WriteLine("unknown card " + cardNameToGain);
                    cardToGain = null;
                }
                else if (cardInfoToGain.Cost > 4)
                {
                    writer.WriteLine("{0} is too expensive to gain", cardInfoToGain.CardName);
                    cardToGain = null;
                }
                else
                {
                    cardToGain = supply.GetCard(cardInfoToGain);
                    if (cardToGain == null)
                    {
                        writer.WriteLine("{0} is not in supply", cardInfoToGain.CardName);
                    }
                }
            } while (cardToGain == null);

            activePlayer.GainCard(cardToGain);
        }
    }
}
