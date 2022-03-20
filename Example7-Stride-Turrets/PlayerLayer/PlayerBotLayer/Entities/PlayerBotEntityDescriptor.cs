using Svelto.ECS.MiniExamples.Turrets.PhysicLayer;

namespace Svelto.ECS.MiniExamples.Turrets.Player
{
    public class PlayerBotEntityDescriptor : ExtendibleEntityDescriptor<PhysicEntityDescriptor>
    {
        public PlayerBotEntityDescriptor()
        {
            ExtendWith<TransformableEntityDescriptor>();
        }
    }
}

