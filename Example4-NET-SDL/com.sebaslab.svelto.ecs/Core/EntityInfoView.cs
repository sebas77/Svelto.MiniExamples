using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    struct EntityInfoComponent: _IInternalEntityComponent
    {
        public IComponentBuilder[] componentsToBuild;
    }
}