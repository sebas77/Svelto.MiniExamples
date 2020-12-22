using Boxtopia.GUIs.LocalisedText;
using Svelto.ECS;

namespace Boxtopia.GUIs.InputField
{
    public class InputFieldEntityDescriptor
        : GenericEntityDescriptor<InputFieldEntityViewComponent, LocalizedLabelEntityViewComponent, EntityHierarchyComponent>
    {}
}