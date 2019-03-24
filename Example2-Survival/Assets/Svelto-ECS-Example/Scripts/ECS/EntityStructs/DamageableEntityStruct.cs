namespace Svelto.ECS.Example.Survive.Characters
{
    struct DamageableEntityStruct:IEntityStruct
    {
        public DamageInfo damageInfo;
        public bool       damaged;
        public EGID       ID { get; set; }
    }
}