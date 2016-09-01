using System;
using DomSample.GameObjects;

namespace DomSample
{
    class Program
    {
        static void Main()
        {
            var game = new Game(
                Console.In, Console.Out,

                new[] { "Wes", "Arden_AI", }, 
                //new[] { "Wes", "Arden", "Becky", "Jimmy", },

                //CardCentral.Shared.GenerateRandomKingdomCardNames(10, true, false)
                new[] { "cellar", "market", "militia", "mine", "moat", "remodel", "smithy", "village", "woodcutter", "workshop", } 
                );
            
            while (game.Progress())
            {
                bool canAcceptMoreInstruction;
                do
                {
                    Console.Write('<');
                    game.DisplayActivePlayerHeader();
                    Console.Write('>');

                    Instruction instruction;
                    if (game.ActivePlayer.PlayerType == PlayerType.AI)
                    {
                        Console.WriteLine("SudoAI");
                        instruction = Instruction.TryParse("SudoAI");
                    }
                    else
                    {
                        instruction = Instruction.TryParse(Console.ReadLine());
                    }
                    
                    if (instruction != null)
                    {
                        game.ApplyInstruction(instruction, out canAcceptMoreInstruction);
                    }
                    else
                    {
                        Console.WriteLine("Unknown instruction");
                        canAcceptMoreInstruction = true;
                    }
                } while (canAcceptMoreInstruction);
            }
            Console.WriteLine("Game ended");
            Console.ReadKey(true);
        }
    }
}
