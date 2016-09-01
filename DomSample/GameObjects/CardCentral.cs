using System;
using System.Collections.Generic;
using DomSample.Utils;

namespace DomSample.GameObjects
{
    /// <summary>
    /// A singleton class that provides various information about cards.
    /// Do not confuse this with Supply, this is not a factory for cards.
    /// </summary>
    public class CardCentral
    {
        #region fields
        private readonly Dictionary<string, CardInfo> cardInfos;
        #endregion

        #region properties
        public static CardCentral Shared
        {
            get { return Singleton<CardCentral>.Instance; }
        }
        #endregion

        #region constructors
        public CardCentral()
        {
            cardInfos = CreateCardInfos();
        }
        #endregion

        #region methods
        public ICardInfo GetCardInfo(string cardName)
        {
            if (string.IsNullOrEmpty(cardName))
                return null;
            
            CardInfo cardInfo;
            if (!cardInfos.TryGetValue(cardName, out cardInfo))
                return null;

            return cardInfo;
        }

        public string[] GenerateRandomKingdomCardNames(int kingdomCardCount, bool needTwoCostCard, bool needDefendCardIfAttack)
        {
            var allKingdoms = GetAllKingdomCardInfos();
            if (kingdomCardCount > allKingdoms.Length)
                throw new ArgumentOutOfRangeException("kingdomCardCount", "there can be maximum " + allKingdoms.Length + " kingdom cards");

            bool hasTwoCostCard;
            bool hasAttackCard;
            bool hasDefendCard;
            do
            {
                allKingdoms.Shuffle(5);

                hasTwoCostCard = false;
                hasAttackCard = false;
                hasDefendCard = false;

                for (int i = 0; i < kingdomCardCount; i++)
                {
                    var kingdom = allKingdoms[i];

                    if (kingdom.Cost == 2)
                        hasTwoCostCard = true;

                    if (kingdom.IsActionCard)
                        hasAttackCard = true;

                    if (kingdom.IsDefendCard)
                        hasDefendCard = true;
                }

            } while ((needTwoCostCard && !hasTwoCostCard) || (hasAttackCard && needDefendCardIfAttack && !hasDefendCard));

            var cardNames = new string[kingdomCardCount];
            for (int i = 0; i < kingdomCardCount; i++)
            {
                cardNames[i] = allKingdoms[i].CardName;
            }
            return cardNames;
        }

        private CardInfo[] GetAllKingdomCardInfos()
        {
            var infos = new List<CardInfo>();
            foreach (var pair in cardInfos)
            {
                if (pair.Value.IsKingdomCard)
                {
                    infos.Add(pair.Value);
                }
            }
            return infos.ToArray();
        }

        private static Dictionary<string, CardInfo> CreateCardInfos()
        {
            var infos = new Dictionary<string, CardInfo>(40, StringComparer.OrdinalIgnoreCase);

            infos.Add("Copper", new CardInfo { Coins = 1, IsTreasureCard = true, });
            infos.Add("Silver", new CardInfo { Coins = 2, Cost = 3, IsTreasureCard = true, });
            infos.Add("Gold", new CardInfo { Coins = 3, Cost = 6, IsTreasureCard = true, });

            infos.Add("Estate", new CardInfo { VictoriyPoints = 1, Cost = 2, IsVictoryCard = true, });
            infos.Add("Duchy", new CardInfo { VictoriyPoints = 3, Cost = 5, IsVictoryCard = true, });
            infos.Add("Province", new CardInfo { VictoriyPoints = 6, Cost = 8, IsVictoryCard = true, });
            infos.Add("Curse", new CardInfo { VictoriyPoints = -1, });

            infos.Add("Adventurer", new CardInfo { Cost = 6, IsActionCard = true, IsKingdomCard = true, });
            infos.Add("Bureaucrat", new CardInfo { Cost = 4, IsActionCard = true, IsKingdomCard = true, IsAttackCard = true,});
            infos.Add("Cellar", new CardInfo { Cost = 2, IsActionCard = true, IsKingdomCard = true, PlusActions = 1, });
            infos.Add("Chancellor", new CardInfo { Cost = 3, IsActionCard = true, IsKingdomCard = true, PlusCoins = 2, });
            infos.Add("Chapel", new CardInfo { Cost = 2, IsActionCard = true, IsKingdomCard = true, });
            infos.Add("Council Room", new CardInfo { Cost = 5, IsActionCard = true, IsKingdomCard = true, PlusCards = 1, PlusBuys = 4, });
            infos.Add("Feast", new CardInfo { Cost = 4, IsActionCard = true, IsKingdomCard = true, });
            infos.Add("Festival", new CardInfo { Cost = 5, IsActionCard = true, IsKingdomCard = true, PlusActions = 2, PlusBuys = 1, PlusCoins = 2, });
            infos.Add("Garden", new CardInfo { Cost = 4, IsVictoryCard = true, IsKingdomCard = true, });
            infos.Add("Laboratory", new CardInfo { Cost = 5, IsActionCard = true, IsKingdomCard = true, PlusActions = 1, PlusCards = 2, });
            infos.Add("Library", new CardInfo { Cost = 5, IsActionCard = true, IsKingdomCard = true, });
            infos.Add("Market", new CardInfo { Cost = 5, IsActionCard = true, IsKingdomCard = true, PlusActions = 1, PlusCards = 1, PlusBuys = 1, PlusCoins = 1, });
            infos.Add("Militia", new CardInfo { Cost = 4, IsActionCard = true, IsKingdomCard = true, IsAttackCard = true, PlusCoins = 2, });
            infos.Add("Mine", new CardInfo { Cost = 5, IsActionCard = true, IsKingdomCard = true, });
            infos.Add("Moat", new CardInfo { Cost = 2, IsActionCard = true, IsKingdomCard = true, IsDefendCard = true, PlusCards = 2,});
            infos.Add("Moneylender", new CardInfo { Cost = 4, IsActionCard = true, IsKingdomCard = true, });
            infos.Add("Remodel", new CardInfo { Cost = 4, IsActionCard = true, IsKingdomCard = true, });
            infos.Add("Smithy", new CardInfo { Cost = 4, IsActionCard = true, IsKingdomCard = true, PlusCards = 3, });
            infos.Add("Spy", new CardInfo { Cost = 4, IsActionCard = true, IsKingdomCard = true, IsAttackCard = true, PlusActions = 1, PlusCards = 1, });
            infos.Add("Thief", new CardInfo { Cost = 4, IsActionCard = true, IsKingdomCard = true, IsAttackCard = true,});
            infos.Add("Throne Room", new CardInfo { Cost = 4, IsActionCard = true, IsKingdomCard = true, });
            infos.Add("Village", new CardInfo { Cost = 3, IsActionCard = true, IsKingdomCard = true, PlusActions = 2, PlusCards = 1, });
            infos.Add("Witch", new CardInfo { Cost = 5, IsActionCard = true, IsKingdomCard = true, IsAttackCard = true, PlusCards = 2, });
            infos.Add("Woodcutter", new CardInfo { Cost = 3, IsActionCard = true, IsKingdomCard = true, PlusBuys = 1, PlusCoins = 2, });
            infos.Add("Workshop", new CardInfo { Cost = 3, IsActionCard = true, IsKingdomCard = true, });

            foreach (var pair in infos)
            {
                pair.Value.CardName = pair.Key;
            }

            return infos;
        }
        #endregion

        #region inner classes
        private class CardInfo : ICardInfo
        {
            #region properties
            public string CardName { get; set; }
            public int VictoriyPoints { get; set; }
            public int Coins { get; set; }
            public int Cost { get; set; }

            public int PlusActions { get; set; }
            public int PlusCards { get; set; }
            public int PlusBuys { get; set; }
            public int PlusCoins { get; set; }

            public bool IsTreasureCard { get; set; }
            public bool IsVictoryCard { get; set; }
            public bool IsActionCard { get; set; }
            public bool IsKingdomCard { get; set; }
            public bool IsAttackCard { get; set; }
            public bool IsDefendCard { get; set; }
            #endregion
        }
        #endregion
    }
}
