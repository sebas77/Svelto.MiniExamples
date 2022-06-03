using System;

namespace  Svelto.ECS.MiniExamples.Doofuses.Stride.StrideLayer
{
    public static class StrideAbstractionContext
    {
        public static void Compose(Action<IEngine> AddEngine, ECSStrideEntityManager ecsStrideEntityManager)
        {
            TransformableContext.Compose(AddEngine);
            
            AddEngine(new SetTransformsEngine(ecsStrideEntityManager));
            AddEngine(new AddStrideEntityToFiltersEngine());
        }
    }
}