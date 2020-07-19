using ServiceLayer;
using Svelto.ECS;

namespace Boxtopia
{
    public struct GameStringEntityComponent : IEntityComponent
    {
        public GameStringsID StringId;
        public EGID          ID { get; set; }
    }
}