namespace Svelto.ECS.MiniExamples.Turrets
{
    class PlayerBotEntityDescriptor : ExtendibleEntityDescriptor<PhysicEntityDescriptor>
    {
        public PlayerBotEntityDescriptor()
        {
            ExtendWith<TransformableEntityDescriptor>();
            Add<EGIDComponent>();
        }
    }
}