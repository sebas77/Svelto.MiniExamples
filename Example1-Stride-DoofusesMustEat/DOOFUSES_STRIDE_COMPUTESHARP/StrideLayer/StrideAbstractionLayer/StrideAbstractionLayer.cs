using System;
using ComputeSharp;

namespace  Svelto.ECS.MiniExamples.Doofuses.StrideExample.StrideLayer
{
    public static class StrideAbstractionContext
    {
        public static void Compose(Action<IEngine> AddEngine, ECSStrideEntityManager ecsStrideEntityManager, GraphicsDevice graphicDevice)
        {
            TransformableContext.Compose(AddEngine, graphicDevice);
            
            AddEngine(new SetTransformsEngine(ecsStrideEntityManager));
            AddEngine(new AddStrideEntityToFiltersEngine());
        }
    }
}