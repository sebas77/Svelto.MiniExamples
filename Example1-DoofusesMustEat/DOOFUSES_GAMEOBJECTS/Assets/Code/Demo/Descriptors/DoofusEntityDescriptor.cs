using Svelto.ECS.EntityComponents;
using Svelto.ECS.Miniexamples.Doofuses.GameObjectsLayer;

namespace Svelto.ECS.Miniexamples.Doofuses.Gameobjects
{
    class DoofusEntityDescriptor
        : GenericEntityDescriptor<PositionEntityComponent, GameObjectEntityComponent, VelocityEntityComponent,
            SpeedEntityComponent, MealInfoComponent>
    {
    }
}