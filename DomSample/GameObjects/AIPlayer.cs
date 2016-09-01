using System;
using System.Collections.Generic;
using System.Text;

namespace DomSample.GameObjects
{
    public class AIPlayer : Player
    {
        #region fields
        private int firstVictoryBuyRound = -1; //-1 means anytime when ok to buy Province
        private Queue<RoundAction> actionStageActions;
        private Queue<RoundAction> buyStageActions;
        
        #endregion

        #region properties
        public override PlayerType PlayerType
        {
            get { return PlayerType.AI; }
        }

        public int FirstVictoryBuyRound
        {
            get { return firstVictoryBuyRound; }
            set { firstVictoryBuyRound = value; }
        }

        public RoundAction CurrentBuyStrategy
        {
            get { return buyStageActions.Peek(); }
        }
        #endregion

        #region constructors
        public AIPlayer(string name, IEnumerable<Card> initialDeckCards):base(name, initialDeckCards)
        {
            actionStageActions = new Queue<RoundAction>();
            buyStageActions = new Queue<RoundAction>();
        }
        #endregion

        #region register methods
        public void AddActionStageStrategy(RoundAction action)
        {
            this.actionStageActions.Enqueue(action);
        }

        public void AddActionStageStrategy(int round, PeformAction action)
        {
            this.AddActionStageStrategy(new RoundAction(round, action));
        }

        public void AddBuyStageStrategy(RoundAction action)
        {
            this.buyStageActions.Enqueue(action);
        }

        public void AddBuyStageStrategy(int round, PeformAction action)
        {
            this.AddBuyStageStrategy(new RoundAction(round, action));
        } 
        #endregion 

        #region ai methods
        public override Instruction GenerateNextInstruction(IGame game)
        {
            if(buyStageActions.Count > 1)
            {
                if (this.ActiveCount > this.CurrentBuyStrategy.Round)
                {
                    this.buyStageActions.Dequeue();
                }
            }

            if (this.CanAction())
            {
                var instruction = GeneralAIHelper.DoAction(this);
                if(instruction != null)
                    return instruction;
            }

            if (this.CanBuy())
            {
                var instruction = this.CurrentBuyStrategy.Action.Invoke(game, this);
                if (instruction != null)
                    return instruction;
            }

            StringBuilder instructionString = new StringBuilder();
            instructionString.Append(InstructionKeyWord.Next);
            return Instruction.TryParse(instructionString.ToString());
        }
        #endregion
    }

    public delegate Instruction PeformAction(IGame game, AIPlayer player);
    public class RoundAction
    {
        public int Round { get; private set; }
        public PeformAction Action { get; private set; }

        public RoundAction(int round, PeformAction action)
        {
            this.Round = round;
            this.Action = action;
        }

        public int ActionCardWeighting { get; set; }
        public int TreasureCardWeighting { get; set; }

        public int MaxActionCards { get; set; }
        public int MaxTreasureCards { get; set; }
    }
}
