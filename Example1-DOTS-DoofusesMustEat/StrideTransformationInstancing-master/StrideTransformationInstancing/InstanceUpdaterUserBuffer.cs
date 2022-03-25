using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using System;
using System.Linq;
using Buffer = Stride.Graphics.Buffer;

namespace StrideTransformationInstancing
{
    public class InstanceUpdaterUserBuffer : InstanceUpdaterBase
    {

        Matrix[] worldInverseTransformations = new Matrix[0];

        Buffer<Matrix> InstanceWorldBuffer;
        Buffer<Matrix> InstanceWorldInverseBuffer;

        InstancingUserBuffer instancingUserBuffer;

        protected override IInstancing GetInstancingType()
        {
            instancingUserBuffer = new InstancingUserBuffer();
            return instancingUserBuffer;
        }

        protected override int InstanceCountSqrt => 20;

        // TODO: make this more easy and clear, improve instancing component to support this better
        protected override void ManageInstancingData()
        {
            var transformUsage = (ModelTransformUsage)(((int)Game.UpdateTime.Total.TotalSeconds) % 3);
            instancingUserBuffer.ModelTransformUsage = ModelTransformUsage.PostMultiply;
            instancingUserBuffer.InstanceCount = instanceWorldTransformations.Length;

            // Make sure inverse matrices are big enough
            if (worldInverseTransformations.Length != instanceWorldTransformations.Length)
            {
                worldInverseTransformations = new Matrix[instanceWorldTransformations.Length];
            }

            // Invert matrices and update bounding box
            var ibb = BoundingBox.Empty;
            for (int i = 0; i < instanceWorldTransformations.Length; i++)
            {
                Matrix.Invert(ref instanceWorldTransformations[i], out worldInverseTransformations[i]);
                var pos = instanceWorldTransformations[i].TranslationVector;
                BoundingBox.Merge(ref ibb, ref pos, out ibb);
            }

            instancingUserBuffer.BoundingBox = ibb;

            // Manage buffers
            if (InstanceWorldBuffer == null || InstanceWorldBuffer.ElementCount < instancingUserBuffer.InstanceCount)
            {
                InstanceWorldBuffer?.Dispose();
                InstanceWorldInverseBuffer?.Dispose();

                InstanceWorldBuffer = CreateMatrixBuffer(GraphicsDevice, instancingUserBuffer.InstanceCount);
                instancingUserBuffer.InstanceWorldBuffer = InstanceWorldBuffer;

                InstanceWorldInverseBuffer = CreateMatrixBuffer(GraphicsDevice, instancingUserBuffer.InstanceCount);
                instancingUserBuffer.InstanceWorldInverseBuffer = InstanceWorldInverseBuffer;
            }

            instancingUserBuffer.InstanceWorldBuffer.SetData(Game.GraphicsContext.CommandList, instanceWorldTransformations);
            instancingUserBuffer.InstanceWorldInverseBuffer.SetData(Game.GraphicsContext.CommandList, worldInverseTransformations);

        }

        private static Buffer<Matrix> CreateMatrixBuffer(GraphicsDevice graphicsDevice, int elementCount)
        {
            return Buffer.New<Matrix>(graphicsDevice, elementCount, BufferFlags.ShaderResource | BufferFlags.StructuredBuffer, GraphicsResourceUsage.Dynamic);
        }
    }
}
