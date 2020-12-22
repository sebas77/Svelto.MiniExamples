using ServiceLayer;

namespace Boxtopia.GUIs.LocalisedText
{
    public interface ILabelText
    {
        GameStringsID textKey { get; }
        string text { set; }
    }
}