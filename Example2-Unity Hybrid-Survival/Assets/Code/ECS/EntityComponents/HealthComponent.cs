namespace Svelto.ECS.Example.Survive.Characters
{
    public struct HealthComponent : IEntityComponent, INeedEGID
    {
        public int  currentHealth;
        
        public EGID ID { get; set; }
    }
}