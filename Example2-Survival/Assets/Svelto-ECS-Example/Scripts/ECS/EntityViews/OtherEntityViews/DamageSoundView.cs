namespace Svelto.ECS.Example.Survive.Characters.Sounds
{
    public struct DamageSoundEntityView: IEntityViewStruct
    {
        public IDamageSoundComponent    audioComponent;
        public EGID ID { get; set; }
    }
}
