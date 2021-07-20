using System;
using Stride.Core.Mathematics;
using Stride.Engine;

namespace Svelto.ECS.MiniExamples.Turrets
{
    class LookAtEngine : SyncScript, IQueryingEntitiesEngine
    {
        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }
        // static readonly Quaternion rotationAxis =
        //     Quaternion.RotationAxis(Vector3.UnitY, MathUtil.DegreesToRadians(180.0f));

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
        // public static void LookAtA(this ref Quaternion e, Vector3 sourcePoint, Vector3 destPoint)
        //     {
        //         Vector3 forwardVector = Vector3.Normalize(destPoint - sourcePoint);
        //         Vector3 cross         = Vector3.Normalize(Vector3.Cross(forwardVector, Vector3.UnitY));
        //         Vector3 U             = Vector3.Cross(cross, forwardVector);                  // rotatedup
        //         
        //         Matrix  m        = new Matrix(); 
        //         m.Row1 = new Vector4(cross, 0);
        //         m.Row2 = new Vector4(U, 0);
        //         m.Row3 = new Vector4(forwardVector, 0);
        //     
        //         Quaternion.RotationMatrix(ref m, out var e);
        //     }
        //
        // public static void LookAtB(this ref Quaternion e, Vector3 sourcePoint, Vector3 destPoint)
        // {
        //     Vector3 forwardVector = Vector3.Normalize(destPoint - sourcePoint);
        //
        //     //compute rotation axis
        //     Vector3 rotAxis = Vector3.Normalize(Vector3.Cross(forwardVector, Vector3.UnitZ));
        //     if (rotAxis.LengthSquared() == 0)
        //         rotAxis = Vector3.UnitY;
        //
        //     //find the angle around rotation axis
        //     float dot = Vector3.Dot(Vector3.UnitZ, forwardVector);
        //     float ang = System.MathF.Cos(dot);
        //
        //     //convert axis angle to quaternion
        //     e = Quaternion.RotationAxis(rotAxis, ang);
        // }

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