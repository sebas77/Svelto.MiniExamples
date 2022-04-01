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
    
    class StrideEntityDescriptor: ExtendibleEntityDescriptor<TransformableEntityDescriptor>
    {
        public StrideEntityDescriptor()
        {
            Add<StrideComponent>();
        }
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

            uint max = 0;
            
            for (int i = 0; i < localFasterReadOnlyList.count; i++)
            {
                if (localFasterReadOnlyList[i].id > max)
                    max = localFasterReadOnlyList[i].id;
            }
            
            max++;

            if (_matrices == null || _matrices.Length != max)
            {
                _matrices = new Matrix[max][];
            }

            for (int i = 0; i < localFasterReadOnlyList.count; i++)
            {
                ExclusiveGroupStruct localFasterReadOnly = localFasterReadOnlyList[i];
                var count               = entitiesDB.Count<MatrixComponent>(localFasterReadOnly);

                if (_matrices[localFasterReadOnly.id] == null || _matrices[localFasterReadOnly.id].Length < count)
                {
                    _matrices[localFasterReadOnly.id] = new Matrix[count];
                }
            }
        }

        public void Step(in float deltaTime)
        {
            var filters = entitiesDB.GetFilters().GetPersistentFilters<StrideComponent>();

            foreach (EntityFilterCollection filtersPerGroup in filters)
            {
                var matrices =
                    _ECSStrideEntityManager.GetInstancingTransformations((uint)filtersPerGroup.combinedFilterID.id);

                if (matrices.Length < filtersPerGroup.count)
                {
                    Array.Resize(ref matrices, filtersPerGroup.count);
                }
                
                foreach (EntityFilterIterator.RefCurrent filterForGroup in filtersPerGroup)
                {
                    var (indices, currentGroup) = filterForGroup;

                    var indicesCount = indices.count;
                    
                    for (int i = 0; i < indicesCount; ++i)
                    {
                        
                    }
                    
                    _ECSStrideEntityManager.SetInstancingTransformations(strideComponents[0].entity,
                        _matrices[currentGroup.id], count);
                }
            }
            
            var groups = entitiesDB.FindGroups<MatrixComponent, StrideComponent>();
            
            foreach (var ((transforms, strideComponents, count), currentGroup) in entitiesDB
                        .QueryEntities<MatrixComponent, StrideComponent>(groups))
            {
                unsafe
                {
                    NB<MatrixComponent> nativeMatrices = transforms;
                    fixed (Matrix* matrices = _matrices[currentGroup.id])
                    {
                        var nativeArray = (void*)nativeMatrices.ToNativeArray(out var _);
                        var size        = (uint)((uint)count * sizeof(Matrix));
                        Unsafe.CopyBlock(matrices, nativeArray, size);
                    }

                   
                }
            }
        }

        readonly ECSStrideEntityManager _ECSStrideEntityManager;
        Matrix[][]                      _matrices;
    }
}