using Svelto.ECS;

namespace Boxtopia.GUIs.Generic
{
    public struct ButtonEntityStruct : IEntityStruct
    {
        public readonly ButtonEvents message;

        public ButtonEntityStruct(EGID egid, ButtonEvents value) : this()
        {
            ID = egid;
            message = value;
        }
        
        public EGID         ID { get; set; }
    }
}