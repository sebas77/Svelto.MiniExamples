using Svelto.ECS;
using Svelto.ECS.Experimental;

namespace User
{
    public struct UserEntityStruct : IEntityStruct
    {
        public ECSString name;
    }
}