using System;
using System.Runtime.CompilerServices;
using Stride.Core.Mathematics;
using Svelto.Common;
using Svelto.Common.Internal;
using Svelto.DataStructures;
using Svelto.ECS.Experimental;

namespace Svelto.ECS.MiniExamples.Doofuses.ComputeSharp.StrideLayer
{
    public enum StrideLayerEngineNames
    {
        SetTransformsEngine
    }

    /// <summary>
    /// Iterate all the entities that have matrices and, assuming they are stride objects, set the matrices to the
    /// matrix to the Stride Entity
    /// </summary>
    [Sequenced(nameof(StrideLayerEngineNames.SetTransformsEngine))]
    class SetTransformsEngine : IQueryingEntitiesEngine, IUpdateEngine, IReactOnSubmission
    {
        public SetTransformsEngine(ECSStrideEntityManager ecsStrideEntityManager)
        {
            _ECSStrideEntityManager = ecsStrideEntityManager;
        }

        public EntitiesDB entitiesDB { get; set; }

        public void Ready()
        {
        }

        public string name => this.TypeName();

        public void EntitiesSubmitted()
        {
            var localFasterReadOnlyList = entitiesDB.FindGroups<MatrixComponent>();

            if (_matrices == null || _matrices.Length != localFasterReadOnlyList.count)
            {
                _matrices = new Matrix[localFasterReadOnlyList.count][];
            }

            for (int i = 0; i < localFasterReadOnlyList.count; i++)
            {
                var count = entitiesDB.Count<MatrixComponent>(localFasterReadOnlyList[i]);

                if (_matrices[i] == null || _matrices[i].Length < count)
                {
                    _matrices[i] = new Matrix[count];
                }
            }
        }

        public void Step(in float deltaTime)
        {
            var groups = entitiesDB.FindGroups<MatrixComponent, StrideComponent>();

            int matrixIndex = 0;

            foreach (var ((transforms, strideComponents, count), currentGroup) in entitiesDB
                        .QueryEntities<MatrixComponent, StrideComponent>(groups))
            {
                unsafe
                {
                    NB<MatrixComponent> nativeMatrices = transforms;
                    fixed (Matrix* matrices = _matrices[matrixIndex])
                    {
                        var nativeArray = (void*)nativeMatrices.ToNativeArray(out var _);
                        var size        = (uint)((uint)count * sizeof(Matrix));
                        Unsafe.CopyBlock(matrices, nativeArray, size);
                    }

                    _ECSStrideEntityManager.SetInstancingTransformations(strideComponents[0].entity,
                        _matrices[matrixIndex], count);

                    matrixIndex++;
                }
            }
        }

        readonly ECSStrideEntityManager _ECSStrideEntityManager;
        Matrix[][]                      _matrices;
    }
}