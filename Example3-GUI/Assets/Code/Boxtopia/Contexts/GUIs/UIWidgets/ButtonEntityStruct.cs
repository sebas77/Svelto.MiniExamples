using Svelto.ECS;

namespace Boxtopia.GUIs.Generic
{
    public struct ButtonEntityComponent : IEntityComponent
    {
        public readonly ButtonEvents message;

        public ButtonEntityComponent(EGID egid, ButtonEvents value) : this()
        {
            ID = egid;
            message = value;
        }
        
        public EGID         ID { get; set; }
    }
}