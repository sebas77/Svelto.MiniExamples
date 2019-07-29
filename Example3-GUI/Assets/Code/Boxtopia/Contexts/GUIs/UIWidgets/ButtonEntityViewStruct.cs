using Svelto.ECS;
using Svelto.ECS.Hybrid;

namespace Boxtopia.GUIs.Generic
{
    public struct ButtonEntityViewStruct : IEntityViewStruct
    {
        public IButtonClick buttonClick;
        public IUIState buttonState;
        
        public EGID ID { get; set; }
    }
}