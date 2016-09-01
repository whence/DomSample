using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DomSample.GameObjects
{
    public class TestAIPlayer : AIPlayer
    {
        public TestAIPlayer(String playerName, IEnumerable<Card> initialCards) :base(playerName, initialCards)
        {
            
        }
    }
}
