using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public abstract class SingleEntityEngine<T> : EngineInfo, IHandleEntityStructEngine<T> where T : IEntityStruct
    {
        public void AddInternal(ref T entityView, ExclusiveGroup.ExclusiveGroupStruct? previousGroup)
        { Add(ref entityView, previousGroup); }

        public void RemoveInternal(ref T entityView)
        { Remove(ref entityView); }

        protected abstract void Add(ref T entityView, ExclusiveGroup.ExclusiveGroupStruct? previousGroup);
        protected abstract void Remove(ref T entityView);
    }
}