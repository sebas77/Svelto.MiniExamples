using System.Collections.Generic;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    public interface IStepEngine : IEngine
    {
        void Step();

        string name { get; }
    }

    public interface IStepEngine<T> : IEngine
    {
        void Step(in T param);

        string name { get; }
    }
    
    public interface IRemovableStepEngine : IStepEngine
    {
        void IStepEngine.Step() { }
        new bool Step();
    }

    public interface IRemovableStepEngine<T> : IStepEngine<T>
    {
        void IStepEngine<T>.Step(in T param) { }
        new bool Step(in T param);
    }

    public interface IGroupEngine
    {
        public IEnumerable<IEngine> engines { get;  }
    }
    
    //this must stay IStepEngine as it may be part of a group itself
    public interface IStepGroupEngine : IStepEngine, IGroupEngine
    {
    }
    
    public interface IStepGroupEngine<T> : IStepEngine<T>, IGroupEngine
    {
    }
}
