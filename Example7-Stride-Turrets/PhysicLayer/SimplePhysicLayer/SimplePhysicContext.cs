using System;
using Svelto.ECS;

namespace Svelto.ECS.MiniExamples.Turrets.PhysicLayer
{
    public class SimplePhysicContext
    {
        public static void Compose(Action<IEngine> AddEngine)
        {
            AddEngine(new VelocityComputationEngine());
            AddEngine(new VelocityToPositionEngine());
        }
    }
}