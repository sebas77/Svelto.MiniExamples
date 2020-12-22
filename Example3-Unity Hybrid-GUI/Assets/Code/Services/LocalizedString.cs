using System.Collections.Generic;

namespace ServiceLayer
{
    internal static class LocalizedString
    {
        public static readonly Dictionary<string, string> LocalizedStrings = new Dictionary<string, string>
        {
            {"strTypeYourDisplayName", "Type a new display name"},
            {"strOK", "Submit"},
            {"strTypeHere", "Type Here"},
            {"strInvalidDisplayName", "Naughty name!"},
            {"strValidDisplayName", "Your name is OK"},
            {"strCancel", "Cancel"},
            {"strQuit", "Quit"},
            {"strRetry", "Retry"},
            {"strBody", "Don't use the word sex in your name otherwise it will be invalid!"},
            {"strSomethingWentWrong", "Internet didn't like it"},
        };
    }
    
    public enum GameStringsID
    {
        NOT_INITIALIZED,

        strTypeYourDisplayName,
        strOK,
        strTypeHere,
        strTranslationNotFound,
        strInvalidDisplayName,
        strValidDisplayName,
        strCancel,
        strQuit,
        strRetry,
        strBody,
        strSomethingWentWrong
    }
}