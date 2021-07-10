using System;
using Stride.Core.Mathematics;
using Stride.Engine;

namespace Svelto.ECS.MiniExamples.Turrets
{
    class PlayerBotLookAtEngine : SyncScript, IQueryingEntitiesEngine
    {
        static readonly Quaternion rotationAxis =
            Quaternion.RotationAxis(Vector3.UnitY, MathUtil.DegreesToRadians(180.0f));

        public override void Update()
        {
            foreach (var ((directions, rotation, count), _) in entitiesDB
               .QueryEntities<DirectionComponent, RotationComponent>(PlayerBotTag.Groups))
            {
                for (int i = 0; i < count; i++)
                {
                    rotation[i].rotation = LookAt(Vector3.Zero, directions[i].vector);
                }
            }
        }

        static Quaternion LookAt(Vector3 sourcePoint, Vector3 destPoint)
        {
            Vector3 forwardVector = Vector3.Normalize(destPoint - sourcePoint);

            float dot = Vector3.Dot(Vector3.UnitZ, forwardVector);

            if (MathF.Abs(dot - (-1.0f)) < 0.000001f)
            {
                return rotationAxis;
            }

            if (MathF.Abs(dot - (1.0f)) < 0.000001f)
            {
                return Quaternion.Identity;
            }

            float   rotAngle = (float) MathF.Acos(dot);
            Vector3 rotAxis  = Vector3.Cross(Vector3.UnitZ, forwardVector);
            rotAxis = Vector3.Normalize(rotAxis);

            return Quaternion.RotationAxis(rotAxis, rotAngle);
        }

        public EntitiesDB entitiesDB { get; set; }
        public void       Ready()    { }
    }
}