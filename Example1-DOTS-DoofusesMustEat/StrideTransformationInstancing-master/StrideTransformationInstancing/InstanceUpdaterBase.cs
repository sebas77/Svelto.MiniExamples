using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using System;
using System.Linq;
using Buffer = Stride.Graphics.Buffer;

namespace StrideTransformationInstancing
{
    public abstract class InstanceUpdaterBase : SyncScript
    {
        protected abstract int InstanceCountSqrt { get; }
        protected Matrix[] instanceWorldTransformations;

        public override void Start()
        {
            var ic = InstanceCountSqrt * InstanceCountSqrt;
            var instancingComponent = Entity.GetOrCreate<InstancingComponent>();
            instancingComponent.Type = GetInstancingType();
            instanceWorldTransformations = new Matrix[ic];
        }

        protected abstract IInstancing GetInstancingType();

        public override void Update()
        {
            UpdateMatrices();

            Entity.Transform.Scale = new Vector3(0.1f);

            ManageInstancingData();
        }

        protected void UpdateMatrices()
        {
            // generate some matrices
            var offset = InstanceCountSqrt / 2;
            var seconds = (float)Game.UpdateTime.Total.TotalSeconds;
            for (int i = 0; i < InstanceCountSqrt; i++)
            {
                var col = i * InstanceCountSqrt;
                for (int j = 0; j < InstanceCountSqrt; j++)
                {
                    var x = i * 1 - offset;
                    var y = j * 1 - offset;
                    var z = (float)Math.Cos(new Vector2(x, y).Length() * 0.5f + seconds);

                    instanceWorldTransformations[col + j] = Matrix.RotationY(seconds) * Matrix.Translation(x, y, z);
                }
            }
        }

        protected abstract void ManageInstancingData();
    }
}
