using System.Collections.Generic;

namespace ServiceLayer
{
    static internal class LocalizedString
    {
        public static readonly Dictionary<string, string> LocalizedStrings = new Dictionary<string, string>
        {
            {"strDisplayNameIsUsed", "This Name is already in use, please choose another."},
            {"strSteamInitializationFailed", "Steam initialization failed"},
            {"strAuthenticationError", "Authentication error"},
            {"strTypeYourDisplayName", "Type a new display name"},
            {"strOK", "OK"},
            {"strTypeHere", "Type Here"},
            {"strInvalidDisplayName", "Invalid display name"},
            {"strTranslationNotFound", "UH!OH! Richard, where is this translation?"},
            {"strValidDisplayName", "I love your name, it's so sexy"},
            {"strServerException", "UH!OH! Servers are a bit down atm, poor servers :("},
            {"strCancel", "Cancel"},
            {"strQuit", "Quit"},
            {"strRetry", "Retry"},
            {"strGenericError", "What's going on?"},
            {"strAlreadyDisplayName", "Name already in use"},
            {"strAreYouSure", "Are you sure?"},
            {"strWannaQuit", "You may be sure, but are you?"},
            {"strPlayGame", "Play Game"},
        };
    }
    
    public enum GameStringsID
    {
        NOT_INITIALIZED,

        strSteamInitializationFailed,
        strAuthenticationError,
        strDisplayNameIsUsed,
        strTypeYourDisplayName,
        strOK,
        strTypeHere,
        strTranslationNotFound,
        strInvalidDisplayName,
        strValidDisplayName,
        strServerException,
        strCancel,
        strQuit,
        strRetry,
        strGenericError,
        strAlreadyDisplayName,
        strAreYouSure,
        strWannaQuit,
        strPlayGame
    }
}