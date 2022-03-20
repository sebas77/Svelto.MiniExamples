using System;

namespace Svelto.ECS.MiniExamples.Turrets
{
    public static class TransformableContext
    {
        public static void Compose(Action<IEngine> AddEngine)
        {
            AddEngine(new LookAtEngine());
            AddEngine(new ComputeTransformsEngine());
            
            //pay attention, I have been lazy here and didn't put in place a sortable group to be sure that
            //ComputeHierarchicalTransformsEngine is always executed after ComputeTransformsEngine
            AddEngine(new ComputeHierarchicalTransformsEngine());
        }
    }
}