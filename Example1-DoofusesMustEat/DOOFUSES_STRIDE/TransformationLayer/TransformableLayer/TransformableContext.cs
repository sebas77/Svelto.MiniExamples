using System;

namespace Svelto.ECS.MiniExamples.Doofuses.Stride
{
    public static class TransformableContext
    {
        public static void Compose(Action<IEngine> AddEngine)
        {
            AddEngine(new ComputeTransformsEngine());
        }
    }
}