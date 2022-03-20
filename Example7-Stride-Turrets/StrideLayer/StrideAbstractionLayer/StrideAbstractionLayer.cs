using System;

namespace Svelto.ECS.MiniExamples.Turrets.StrideLayer
{
    public static class StrideAbstractionContext
    {
        public static void Compose(Action<IEngine> AddEngine, ECSStrideEntityManager ecsStrideEntityManager)
        {
            AddEngine(new SetTransformsEngine(ecsStrideEntityManager));
        }
    }
}