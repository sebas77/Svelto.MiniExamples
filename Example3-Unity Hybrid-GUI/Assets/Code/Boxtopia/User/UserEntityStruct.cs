using Svelto.ECS;
using Svelto.ECS.Experimental;

namespace User
{
    public struct UserEntityComponent : IEntityComponent
    {
        public ECSString name;
    }
}