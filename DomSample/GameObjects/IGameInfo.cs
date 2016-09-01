using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DomSample.GameObjects
{
    public interface IGameInfo
    {
        IDictionary<String, ICardInfo> KingdomCards { get; }
        IDictionary<String, ICardInfo> ActionCards { get; }
        IDictionary<String, ICardInfo> AttackCards { get; }
        IDictionary<String, ICardInfo> ReactionCards { get; }
        IDictionary<String, ICardInfo> VictoryCards { get; }
        IDictionary<String, ICardInfo> TreasureCards { get; }
    }
}
