using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DomSample.GameObjects
{
    public interface IGame
    {
        IEnumerable<Player> Players { get; }
        IGameInfo GameInfo { get; }
        IDictionary<string, CardPile> CardPiles { get; }
    }
}
