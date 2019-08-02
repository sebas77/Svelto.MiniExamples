using System.Collections.Generic;

namespace ServiceLayer
{
    internal static class LocalizedString
    {
        public static readonly Dictionary<string, string> LocalizedStrings = new Dictionary<string, string>
        {
            {"strDisplayNameIsUsed", "This Name is already in use, please choose another."},
            {"strTypeYourDisplayName", "Type a new display name"},
            {"strOK", "OK"},
            {"strTypeHere", "Type Here"},
            {"strInvalidDisplayName", "Invalid display name"},
            {"strValidDisplayName", "I love your name, it's so sexy"},
            {"strCancel", "Cancel"},
            {"strQuit", "Quit"},
            {"strRetry", "Retry"},
            {"strAlreadyDisplayName", "Name already in use"},
        };
    }
    
    public enum GameStringsID
    {
        NOT_INITIALIZED,

        strDisplayNameIsUsed,
        strTypeYourDisplayName,
        strOK,
        strTypeHere,
        strTranslationNotFound,
        strInvalidDisplayName,
        strValidDisplayName,
        strCancel,
        strQuit,
        strRetry,
        strAlreadyDisplayName,
    }
}