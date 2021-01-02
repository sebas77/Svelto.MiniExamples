namespace Svelto.ECS.Example.Survive.Characters.Player
{
    public readonly struct SpeedComponent : IEntityComponent
    {
        public readonly float movementSpeed;

        public SpeedComponent(float speed)
        {
            movementSpeed = speed;
        }
    }
}