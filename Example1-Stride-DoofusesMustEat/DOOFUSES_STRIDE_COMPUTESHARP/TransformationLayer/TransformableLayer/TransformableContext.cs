using System;
using ComputeSharp;

namespace Svelto.ECS.MiniExamples.Doofuses.StrideExample
{
    public static class TransformableContext
    {
        public static void Compose(Action<IEngine> AddEngine, GraphicsDevice graphicsDevice)
        {
            AddEngine(new ComputeTransformsEngine(graphicsDevice));
        }
    }
}