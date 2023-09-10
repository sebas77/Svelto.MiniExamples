using Svelto.ECS;
using Svelto.ECS.Experimental;
using Svelto.ECS.ResourceManager;

namespace User
{
    public struct UserEntityComponent : IEntityComponent
    {
        public ECSString name;
    }
}