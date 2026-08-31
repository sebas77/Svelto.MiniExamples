using Boxtopia.GUIs.LocalisedText;
using Svelto.ECS;

namespace Boxtopia.GUIs.NameValidation
{
    public class NameValidationFeedbackLabelEntityDescriptor : GenericEntityDescriptor<
        NameValidationFeedbackLabelViewStruct, LocalizedLabelEntityViewComponent>
    {}

    public struct NameValidationFeedbackLabelViewStruct : IEntityComponent
    {}
}
