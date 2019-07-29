using ServiceLayer;
using Svelto.ECS;

namespace Boxtopia
{
    public struct GameStringEntityStruct : IEntityStruct
    {
        public GameStringsID StringId;
        public EGID          ID { get; set; }
    }
}