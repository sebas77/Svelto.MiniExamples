namespace Svelto.ECS
{
    public struct EntityReferenceComponent:IEntityComponent, INeedEntityReference
    {
        public EntityReference selfReference { get; set; }
    }
}