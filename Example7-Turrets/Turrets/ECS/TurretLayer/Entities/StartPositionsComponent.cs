using Vector3 = Stride.Core.Mathematics.Vector3;

namespace Svelto.ECS.MiniExamples.Turrets
{
    public readonly struct StartPositionsComponent : IEntityComponent
    {
        public StartPositionsComponent(Vector3 startPosition)
        {
            this.startPosition    = startPosition;
        }

        public readonly Vector3 startPosition;
    }
    
    public readonly struct TurretTargetComponent : IEntityComponent
    {
    }
}