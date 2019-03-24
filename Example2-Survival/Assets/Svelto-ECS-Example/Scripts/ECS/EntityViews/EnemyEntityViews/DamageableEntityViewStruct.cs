namespace Svelto.ECS.Example.Survive.Characters.Enemies
{
    public struct DamageableEntityViewStruct : IEntityViewStruct
    {
        public DamageInfo         damageInfo;
        public EGID               ID { get; set; }
    }
}