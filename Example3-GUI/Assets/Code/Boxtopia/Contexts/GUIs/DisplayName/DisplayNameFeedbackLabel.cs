using Boxtopia.GUIs.LocalisedText;
using Svelto.ECS;

namespace Boxtopia.GUIs.DisplayName
{
    public class DisplayNameFeedbackLabelEntityDescriptor : GenericEntityDescriptor<
        DisplayNameFeedbackLabelViewStruct,
        LocalizedLabelEntityViewStruct>
    {}

    public struct DisplayNameFeedbackLabelViewStruct : IEntityStruct
    {}
}
