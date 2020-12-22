namespace Svelto.ECS.Example.Survive.Characters
{
    struct DamageableComponent : IEntityComponent, INeedEGID
    {
        public DamageInfo damageInfo;
        
        public EGID ID { get; set; }
    }
}