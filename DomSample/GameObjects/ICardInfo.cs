namespace DomSample.GameObjects
{
    public interface ICardInfo
    {
        string CardName { get; }

        int VictoriyPoints { get; }
        int Coins { get; }
        int Cost { get; }

        bool IsTreasureCard { get; }
        bool IsVictoryCard { get; }
        bool IsActionCard { get; }
        bool IsKingdomCard { get; }

        int PlusActions { get; }
        int PlusCards { get; }
        int PlusBuys { get; }
        int PlusCoins { get; }

        // this is not currently used outside supply generation. the actual attacks are implemented in resolution codes
        bool IsAttackCard { get; }

        // defend card means it can immune holder from attack. 
        // it's just one kind of reaction card, though in base game, all reaction cards are defend cards
        bool IsDefendCard { get; }
    }
}
