using Stride.Engine;
using Svelto.ECS.MiniExamples.Turrets.EnemyLayer;
using Svelto.ECS.MiniExamples.Turrets.StrideLayer;

namespace Svelto.ECS.MiniExamples.Turrets
{
    /// <summary>
    /// This may evolve in something similar to the GenericEntityDescriptorHolder if makes sense
    /// </summary>
    [SveltoEntityComponentProcessor(typeof(TurretEntityHolderComponentProcessor))]
    public class TurretEntityHolder : ScriptComponent
    {
        public TurretBaseEntityDescriptor GetDescriptor()
        {
            return EntityDescriptorTemplate<TurretBaseEntityDescriptor>.realDescriptor;
        }

        public TurretTopEntityDescriptor GetChildDescriptor()
        {
            return EntityDescriptorTemplate<TurretTopEntityDescriptor>.realDescriptor;
        }

        public Entity child;
    }
}