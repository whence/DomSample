using System;
using System.Collections.Generic;
using System.IO;
using DomSample.Utils;

namespace DomSample.GameObjects
{
    public partial class Game : IGame
    {
        #region constants
        private const string StandardErrorMessageForInstruction = "Invalid instruction";
        #endregion

        #region fields
        private readonly TextReader reader;
        private readonly TextWriter writer;

        private readonly GameInfo gameInfo;
        private readonly Supply supply;

        private readonly List<Player> players;
        private int activePlayerIndex;

        private readonly List<Card> trashCards;

        private readonly Dictionary<string, Action<Player, Card>> cardResolvers;

        private bool gameEnded;
        #endregion

        #region IGame Interface
        public IEnumerable<Player> Players
        {
            get { return players; }
        }

        public IGameInfo GameInfo
        {
            get { return gameInfo; }
        }

        public Player ActivePlayer
        {
            get { return players[activePlayerIndex]; }
        }

        public IDictionary<string, CardPile> CardPiles
        {
            get { return supply.CardPiles; }
        }
        #endregion

        #region constructors
        public Game(TextReader reader, TextWriter writer, ICollection<string> playerNames, ICollection<string> kingdomCardNames)
        {
            this.reader = reader;
            this.writer = writer;

            gameInfo = new GameInfo(kingdomCardNames);
            supply = new Supply(playerNames.Count, kingdomCardNames);

            players = new List<Player>(playerNames.Count);
            foreach (var playerName in playerNames)
            {
                var initialDeckCards = new List<Card>(10);
                initialDeckCards.AddRange(supply.GetCards(CardCentral.Shared.GetCardInfo("Estate"), 3));
                initialDeckCards.AddRange(supply.GetCards(CardCentral.Shared.GetCardInfo("Copper"), 7));

                if (playerName.EndsWith("_AI"))
                {
                    var aiPlayer = new AIPlayer(playerName, initialDeckCards);
                    aiPlayer.AddBuyStageStrategy(new RoundAction(0, BuyStrategy.WeightingBuy)
                                           {
                                               ActionCardWeighting = 10,
                                               TreasureCardWeighting = 80,
                                               MaxTreasureCards = 15,
                                               MaxActionCards = 1,
                                           });
                    
                    aiPlayer.FirstVictoryBuyRound = 12;
                    players.Add(aiPlayer);
                }
                else
                    players.Add(new Player(playerName, initialDeckCards));
            }
            activePlayerIndex = -1;

            trashCards = new List<Card>();

            cardResolvers = CreateCardResolvers();
        }
        #endregion

        #region game methods
        public void DisplayActivePlayerHeader()
        {
            var activePlayer = players[activePlayerIndex];
            activePlayer.DisplayHeader(writer);
        }

        public bool Progress()
        {
            if (gameEnded)
                return false;
            
            if (players.Count <= 0)
                return false;

            activePlayerIndex = Maths.StepInLoop(activePlayerIndex, 1, 0, players.Count - 1);
            var activePlayer = players[activePlayerIndex];
            activePlayer.BecomeActive();
            activePlayer.DisplayInfo(writer);

            return true;
        }

        public void ApplyInstruction(Instruction instruction, out bool canAcceptMoreInstruction)
        {
            var activePlayer = players[activePlayerIndex];
            
            switch (instruction.Keyword)
            {
                case InstructionKeyWord.Play:
                    if (!string.IsNullOrEmpty(instruction.CardName))
                    {
                        if (activePlayer.CanAction())
                        {
                            var cardToPlay = activePlayer.FindCardOnHand(instruction.CardName);
                            if (cardToPlay != null)
                            {
                                if (cardToPlay.Info.IsActionCard)
                                {
                                    activePlayer.PlayActionCard(cardToPlay);
                                    writer.WriteLine("{0} played {1}", activePlayer.Name, cardToPlay.Info.CardName);
                                    
                                    Action<Player, Card> cardResolver;
                                    if (!cardResolvers.TryGetValue(cardToPlay.Info.CardName, out cardResolver))
                                        throw new Exception(string.Format("Action card {0} has no resolver", cardToPlay.Info.CardName));

                                    cardResolver(activePlayer, cardToPlay);
                                    activePlayer.DisplayInfo(writer);
                                }
                                else
                                {
                                    writer.WriteLine("{0} is not an action card", cardToPlay.Info.CardName);
                                }
                            }
                            else
                            {
                                writer.WriteLine("You don't have {0} on hand", instruction.CardName);
                            }
                        }
                        else
                        {
                            writer.WriteLine("Cannot action anymore in this round");
                        }
                    }
                    else
                    {
                        writer.WriteLine(StandardErrorMessageForInstruction);
                    }

                    canAcceptMoreInstruction = true;
                    break;
                
                case InstructionKeyWord.Buy:
                    if (!string.IsNullOrEmpty(instruction.CardName))
                    {
                        if (activePlayer.CanBuy())
                        {
                            var cardInfo = CardCentral.Shared.GetCardInfo(instruction.CardName);
                            if (cardInfo != null)
                            {
                                if (activePlayer.HasCoins(cardInfo.Cost))
                                {
                                    var card = supply.GetCard(cardInfo);
                                    if (card != null)
                                    {
                                        activePlayer.BuyCard(card);
                                        writer.WriteLine("{0} bought {1} for {2} coins", activePlayer.Name, card.Info.CardName, card.Info.Cost);
                                        activePlayer.DisplayInfo(writer);
                                    }
                                    else
                                    {
                                        writer.WriteLine("Cannot find {0} in supply", cardInfo.CardName);
                                    }
                                }
                                else
                                {
                                    writer.WriteLine("Not enough coins to buy {0}", cardInfo.CardName);
                                }
                            }
                            else
                            {
                                writer.WriteLine("Unknown card: {0}", instruction.CardName);
                            }
                        }
                        else
                        {
                            writer.WriteLine("Cannot buy anymore in this round");
                        }
                    }
                    else
                    {
                        writer.WriteLine(StandardErrorMessageForInstruction);
                    }

                    canAcceptMoreInstruction = true;
                    break;
                
                case InstructionKeyWord.Next:
                    activePlayer.Cleanup();

                    if (supply.IsDry())
                    {
                        EndGame();
                    }

                    canAcceptMoreInstruction = false;
                    break;

                case InstructionKeyWord.Info:
                    activePlayer.DisplayInfo(writer);
                    canAcceptMoreInstruction = true;
                    break;

                case InstructionKeyWord.Supply:
                    supply.Display(writer);
                    canAcceptMoreInstruction = true;
                    break;

                case InstructionKeyWord.Quit:
                case InstructionKeyWord.Exit:
                    EndGame();
                    canAcceptMoreInstruction = false;
                    break;

                case InstructionKeyWord.SudoAI:
                    var nextInstruction = activePlayer.GenerateNextInstruction(this);
                    this.ApplyInstruction(nextInstruction, out canAcceptMoreInstruction);
                    break;

                case InstructionKeyWord.CheatDeck:
                    activePlayer.DisplayDeck(writer);
                    canAcceptMoreInstruction = true;
                    break;

                default:
                    writer.WriteLine(StandardErrorMessageForInstruction);
                    canAcceptMoreInstruction = true;
                    break;
            }

            writer.WriteLine();
        }

        public void EndGame()
        {
            var playerStats = CreatePlayerStats();
            Array.Sort(playerStats, PlayerStat.ComparePlayerStatFromGoodToBad);
            writer.WriteLine("Game ends with the following stats:");
            PlayerStat topPlayerStat = null;
            foreach (var playerStat in playerStats)
            {
                writer.Write(playerStat.Player.Name);
                writer.Write("  Victory Points:");
                writer.Write(playerStat.TotalVictoryPoints);
                writer.Write("  Turns:");
                writer.Write(playerStat.Player.ActiveCount);

                bool isWinner;
                if (topPlayerStat != null)
                {
                    switch (playerStat.CompareWith(topPlayerStat))
                    {
                        case PlayerStatComparison.Better:
                        case PlayerStatComparison.Equal:
                            isWinner = true;
                            break;

                        default:
                            isWinner = false;
                            break;
                    }
                }
                else
                {
                    topPlayerStat = playerStat;
                    isWinner = true;
                }
                if (isWinner)
                {
                    writer.Write("  [Winner]");
                }

                writer.WriteLine();
            }

            gameEnded = true;
        }

        private PlayerStat[] CreatePlayerStats()
        {
            var playerStats = new PlayerStat[players.Count];
            for (int i = 0; i < players.Count; i++)
            {
                var player = players[i];
                playerStats[i] = new PlayerStat(player, player.CalculateTotalVictoryPoints());
            }
            return playerStats;
        }
        #endregion

        #region shared methods
        private bool TryDefendAttack(Player player)
        {
            var defendCard = player.FindDefendCardOnHand();
            if (defendCard == null)
                return false;
            
            writer.Write("{0}, would you like to reveal reaction card to defend this attack?", player.Name);
            if (Texts.ParseBoolean(reader.ReadLine()) ?? true)
            {
                player.RevealHandCard(defendCard);
                writer.WriteLine("{0} revealed {1} to defend attack", player.Name, defendCard.Info.CardName);
                player.UnrevealCardToHand(defendCard);
                
                return true;
            }
            return false;
        }
        #endregion

        #region inner classes
        private class PlayerStat
        {
            #region fields
            private readonly Player player;
            private readonly int totalVictoryPoints;
            #endregion

            #region properties
            public Player Player
            {
                get { return player; }
            }

            public int TotalVictoryPoints
            {
                get { return totalVictoryPoints; }
            }
            #endregion

            #region constructors
            public PlayerStat(Player player, int totalVictoryPoints)
            {
                this.player = player;
                this.totalVictoryPoints = totalVictoryPoints;
            }
            #endregion

            #region methods
            public PlayerStatComparison CompareWith(PlayerStat otherStat)
            {
                if (this.TotalVictoryPoints > otherStat.TotalVictoryPoints)
                    return PlayerStatComparison.Better;

                if (this.TotalVictoryPoints < otherStat.TotalVictoryPoints)
                    return PlayerStatComparison.Worse;

                if (this.Player.ActiveCount < otherStat.Player.ActiveCount)
                    return PlayerStatComparison.Better;

                if (this.Player.ActiveCount > otherStat.Player.ActiveCount)
                    return PlayerStatComparison.Worse;

                return PlayerStatComparison.Equal;
            }

            public static int ComparePlayerStatFromGoodToBad(PlayerStat x, PlayerStat y)
            {
                int notionResult; // i.e. the result of the normal ascend order 
                switch (x.CompareWith(y))
                {
                    case PlayerStatComparison.Better:
                        notionResult = 1;
                        break;

                    case PlayerStatComparison.Worse:
                        notionResult = -1;
                        break;

                    default:
                        notionResult = 0;
                        break;
                }

                const int direction = -1; // we need a descend order
                return notionResult * direction;
            }
            #endregion
        }

        private enum PlayerStatComparison
        {
            Better,
            Equal,
            Worse,
        }
        #endregion
    }
}
