namespace Svelto.ECS
{
    struct EntityInfoComponentView: IEntityComponent
    {
        public IEntityComponentBuilder[] componentsToBuild;
    }
}