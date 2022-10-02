namespace Svelto.ECS.Example.Survive.Player
{
    public class SyncAnimationEngine : IReactOnSubmission, IQueryingEntitiesEngine
    {
        public void EntitiesSubmitted()
        {
            throw new System.NotImplementedException();
        }

        public void Ready()
        {
            throw new System.NotImplementedException();
        }

        public EntitiesDB entitiesDB { get; set; }
    }
}