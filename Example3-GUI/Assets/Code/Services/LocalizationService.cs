using System;
using System.Collections.Generic;

///Mockup To do

namespace ServiceLayer
{
    public static class LocalizationService
    {
        public static string Localize(GameStringsID entityStringIdId)
        {
            return LocalizedString.LocalizedStrings[GameStringIDsToString[entityStringIdId]];
        }

        public static GameStringsID VerySlowParseEnum(string value)
        {
            return Enum.TryParse(value, true, out GameStringsID result) == false ? GameStringsID.strTranslationNotFound : result;
        }

        static readonly Dictionary<GameStringsID, string> GameStringIDsToString = new Dictionary<GameStringsID, string>();

        static LocalizationService()
        {
            foreach (var str in Enum.GetValues(typeof(GameStringsID)))
            {
                GameStringIDsToString[(GameStringsID) str] = str.ToString();
            }
        }
    }
}