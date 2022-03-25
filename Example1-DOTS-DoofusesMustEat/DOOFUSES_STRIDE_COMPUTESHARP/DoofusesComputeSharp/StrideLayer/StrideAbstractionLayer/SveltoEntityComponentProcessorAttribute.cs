using System;
using Stride.Engine.Design;

namespace  Svelto.ECS.MiniExamples.Doofuses.ComputeSharp.StrideLayer
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class SveltoEntityComponentProcessorAttribute : DefaultEntityComponentProcessorAttribute
    {
        public SveltoEntityComponentProcessorAttribute(Type type) : base(type)
        {
            ExecutionMode = ExecutionMode.Runtime;
        }
    }
}