using System.Runtime.CompilerServices;
using ComputeSharp;
using Stride.Core.Mathematics;
using Svelto.Common;
using Svelto.Common.Internal;

namespace  Svelto.ECS.MiniExamples.Doofuses.StrideExample
{
    public enum TransformableLayerEngineNames
    {
        ComputeTransformsEngine
    }
    //todo I wanted to abstract too much here, Better to remove it and remove the matrix component
    /// <summary>
    /// Iterate all the entities that can be transformed and compute the transformation matrices out the transformation
    /// parameters
    /// </summary>
    [Sequenced(nameof(TransformableLayerEngineNames.ComputeTransformsEngine))]
    class ComputeTransformsEngine : IQueryingEntitiesEngine, IUpdateEngine
    {
        public ComputeTransformsEngine(GraphicsDevice graphicsDevice) 
        {
            _graphicsDevice = graphicsDevice;
        }
        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        public string name => nameof(ComputeTransformsEngine);
        
        public void Step(in float deltaTime)
        {
            var groups =
                entitiesDB.FindGroups<ComputePositionComponent, ComputeRotationComponent, ComputeMatrixComponent>();

            foreach (var ((positions, rotation, transforms, count), _) in entitiesDB
               .QueryEntities<ComputePositionComponent, ComputeRotationComponent, ComputeMatrixComponent>(groups))
            {
#if COMPUTE_SHADERS                
                _graphicsDevice.For(
                    count,
                    new ComputeMatricesJob(
                        (positions.ToComputeBuffer(), rotation.ToComputeBuffer(), transforms.ToComputeBuffer()), deltaTime));
                
                transforms.Update();
#else                
                for (int index = 0; index < count; index++)
                {
                    ComputeMatricesJob.Transformation(ref rotation[index].rotation, ref positions[index].position, out transforms[index].matrix);
                }
#endif
                
            }
        }
        
        readonly GraphicsDevice _graphicsDevice;
    }
    
    [AutoConstructor]
    readonly partial struct ComputeMatricesJob: IComputeShader
    {
        public ComputeMatricesJob((ReadWriteBuffer<ComputePositionComponent> positions, ReadWriteBuffer<ComputeRotationComponent> rotations, 
                            ReadWriteBuffer<ComputeMatrixComponent> matrices) doofuses,
            float deltaTime)
        {
            _positions = doofuses.positions;
            _rotations = doofuses.rotations;
            _matrices = doofuses.matrices;
            _deltaTime = deltaTime;
        }

        public void Execute()
        {
            var index = ThreadIds.X;
            
            Transformation(ref _rotations[index].rotation, ref _positions[index].position, out _matrices[index].matrix);
        }

        public static void Transformation(ref Quaternion rotation, ref Vector3 translation, out Matrix result)
        {
            // Equivalent to:
            //result =
            //    Matrix.Scaling(scaling)
            //    *Matrix.RotationX(rotation.X)
            //    *Matrix.RotationY(rotation.Y)
            //    *Matrix.RotationZ(rotation.Z)
            //    *Matrix.Position(translation);

            // Rotation
            float xx = rotation.X * rotation.X;
            float yy = rotation.Y * rotation.Y;
            float zz = rotation.Z * rotation.Z;
            float xy = rotation.X * rotation.Y;
            float zw = rotation.Z * rotation.W;
            float zx = rotation.Z * rotation.X;
            float yw = rotation.Y * rotation.W;
            float yz = rotation.Y * rotation.Z;
            float xw = rotation.X * rotation.W;

            result.M11 = 1.0f - (2.0f * (yy + zz));
            result.M12 = 2.0f * (xy + zw);
            result.M13 = 2.0f * (zx - yw);
            result.M21 = 2.0f * (xy - zw);
            result.M22 = 1.0f - (2.0f * (zz + xx));
            result.M23 = 2.0f * (yz + xw);
            result.M31 = 2.0f * (zx + yw);
            result.M32 = 2.0f * (yz - xw);
            result.M33 = 1.0f - (2.0f * (yy + xx));

            // Position
            result.M41 = translation.X;
            result.M42 = translation.Y;
            result.M43 = translation.Z;

            result.M14 = 0.0f;
            result.M24 = 0.0f;
            result.M34 = 0.0f;
            result.M44 = 1.0f;
        }

        readonly float _deltaTime;

        readonly ReadWriteBuffer<ComputePositionComponent> _positions;
        readonly ReadWriteBuffer<ComputeRotationComponent> _rotations;
        readonly ReadWriteBuffer<ComputeMatrixComponent> _matrices;
    }
}