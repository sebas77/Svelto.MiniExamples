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