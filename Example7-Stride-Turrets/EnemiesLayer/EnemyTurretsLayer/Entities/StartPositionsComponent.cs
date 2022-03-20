using Vector3 = Stride.Core.Mathematics.Vector3;

namespace Svelto.ECS.MiniExamples.Turrets.EnemyLayer
{
    public readonly struct StartPositionsComponent : IEntityComponent
    {
        public StartPositionsComponent(Vector3 startPosition)
        {
            this.startPosition    = startPosition;
        }

        public readonly Vector3 startPosition;
    }
}