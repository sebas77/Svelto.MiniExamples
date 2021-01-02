namespace Svelto.ECS.Example.Survive.Characters
{
    public struct DamageableComponent : IEntityComponent, INeedEGID
    {
        public DamageInfo damageInfo;
        
        public EGID ID { get; set; }
    }
}