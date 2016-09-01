using System;
using DomSample.Utils;

namespace DomSample.GameObjects
{
    public class Instruction
    {
        #region properties
        public InstructionKeyWord Keyword { get; private set; }
        public string CardName { get; set; }
        #endregion

        #region factory methods
        public static Instruction TryParse(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            string keywordPart;
            string cardNamePart;
            var splitterIndex = text.IndexOf(Chars.Space);
            if (splitterIndex == -1)
            {
                keywordPart = text.Trim();
                cardNamePart = null;
            }
            else
            {
                keywordPart = text.Substring(0, splitterIndex);
                cardNamePart = text.Substring(splitterIndex + 1, text.Length - splitterIndex - 1).Trim();
                if (string.IsNullOrEmpty(cardNamePart))
                    cardNamePart = null;
            }

            InstructionKeyWord keyword;
            if (!Enum.TryParse(keywordPart, true, out keyword))
                return null;

            return new Instruction {Keyword = keyword, CardName = cardNamePart,};
        }
        #endregion
    }
}
