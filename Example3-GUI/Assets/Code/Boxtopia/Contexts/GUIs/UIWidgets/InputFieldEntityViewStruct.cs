using Svelto.ECS;
using Svelto.ECS.Hybrid;

namespace Boxtopia.GUIs.InputField
{
    public struct InputFieldEntityViewStruct : IEntityViewStruct
    {
        public EGID ID { get; set; }

        public IInputField inputField;
    }

    public interface IInputField
    {
        string text { get; }
        int limit { set; get; }
    }
}    