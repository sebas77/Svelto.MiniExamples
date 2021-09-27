using System;
using Stride.Core.Mathematics;
using Stride.Engine;
using Svelto.Common.Internal;

namespace Svelto.ECS.MiniExamples.Turrets
{
    /// <summary>
    /// Should LookAtEngine be abstract or not? If it's abstract DirectionComponent cannot be provided by
    /// the PhysicLayer otherwise LookAtEngine would be more specialised than it.
    /// Alternatively it should have a LookAtComponent. I went for the latter.
    /// </summary>
    class LookAtEngine : IQueryingEntitiesEngine, IUpdateEngine
    {
        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        public string name => this.TypeName();
        
        public void Step(in float deltaTime)
        {
            var groups = entitiesDB.FindGroups<LookAtComponent, RotationComponent>();

            foreach (var ((directions, rotation, count), _) in entitiesDB
               .QueryEntities<LookAtComponent, RotationComponent>(groups))
            {
                for (int i = 0; i < count; i++)
                {
                    rotation[i].rotation.LookAt(directions[i].vector, Vector3.Zero);
                }
            }
        }
    }

    public static class EntityExtensions
    {
        public static void LookAt(this ref Quaternion e, Vector3 sourcePoint, Vector3 destPoint)
        {
            float altitude = 0;
            float azimuth  = GetLookAtAngles(sourcePoint, destPoint, out altitude);
            var   result   = Quaternion.RotationYawPitchRoll(azimuth, -altitude, 0);
            e = result;
        }

        static float GetLookAtAngles(Vector3 source, Vector3 destination, out float altitude)
        {
            var x = source.X - destination.X;
            var y = source.Y - destination.Y;
            var z = source.Z - destination.Z;

            altitude = (float)Math.Atan2(y, Math.Sqrt(x * x + z * z));
            var azimuth = (float)Math.Atan2(x, z);
            return azimuth;
        }
    }
}