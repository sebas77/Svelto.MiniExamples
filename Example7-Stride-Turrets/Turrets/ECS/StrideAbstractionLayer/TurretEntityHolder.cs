using Stride.Engine;

namespace Svelto.ECS.MiniExamples.Turrets
{
    /// <summary>
    /// This may evolve in something similar to the GenericEntityDescriptorHolder if makes sense
    /// </summary>
    [SveltoEntityComponentProcessorAttribute(typeof(TurretEntityHolderComponentProcessor))]
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