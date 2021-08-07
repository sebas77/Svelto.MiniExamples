using System;
using Stride.Core.Mathematics;
using Stride.Engine;

namespace Svelto.ECS.MiniExamples.Turrets
{
    class LookAtEngine : SyncScript, IQueryingEntitiesEngine
    {
        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        public override void Update()
        {
            var groups = entitiesDB.FindGroups<DirectionComponent, RotationComponent>();

            foreach (var ((directions, rotation, count), _) in entitiesDB
               .QueryEntities<DirectionComponent, RotationComponent>(groups))
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