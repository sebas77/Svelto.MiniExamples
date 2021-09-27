namespace Svelto.ECS
{
    public interface IGetReadyEngine : IEngine
    {
        void Ready();
    }
    
    public interface IQueryingEntitiesEngine : IGetReadyEngine
    {
        EntitiesDB entitiesDB { set; }

        void Ready();
    }
}