using System;

namespace Svelto.ECS.MiniExamples.Doofuses.StrideExample
{
    public static class TransformableContext
    {
        public static void Compose(Action<IEngine> AddEngine)
        {
            AddEngine(new ComputeTransformsEngine());
        }
    }
}