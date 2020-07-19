using Svelto.ECS;
using Svelto.ECS.Hybrid;

namespace Boxtopia.GUIs.Generic
{
    public struct ButtonEntityViewComponent : IEntityViewComponent
    {
        public IButtonClick buttonClick;
        public IUIState buttonState;
        
        public EGID ID { get; set; }
    }
}