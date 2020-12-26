namespace Svelto.ECS
{
    public interface IStepEngine : IEngine
    {
        void Step();
        
        string name { get; }
    }
    
    public interface IStepEngine<T> : IEngine
    {
        void StepAll(in T _param);
        
        string name { get; }
    }
    
    public interface IStepGroupEngine : IEngine
    {
        void StepAll();
        
        string name { get; }
    }
    
    public interface IStepGroupEngine<T> : IStepEngine<T>
    {
    }
}