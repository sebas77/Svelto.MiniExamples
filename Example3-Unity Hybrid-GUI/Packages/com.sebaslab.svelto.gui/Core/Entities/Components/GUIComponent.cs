using Svelto.ECS.GUI.Resources;

namespace Svelto.ECS.GUI
{
    public struct GUIComponent : IEntityComponent, INeedEntityReference
    {
        public bool        isRoot;

        public EcsResource root;
        public EcsResource name;
        public EcsResource container;

        public bool        isDirty;

        public EntityReference selfReference { get; set; }
    }
}