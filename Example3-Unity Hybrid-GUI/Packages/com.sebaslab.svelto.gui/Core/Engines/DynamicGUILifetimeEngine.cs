namespace Svelto.ECS.GUI.Engines
{
    class DynamicGUILifetimeEngine : IReactOnRemove<RecyclableGUIComponent>, IReactOnDispose<RecyclableGUIComponent>
    {
        public DynamicGUILifetimeEngine(IGUIBuilder builder)
        {
            _builder = builder;
        }

        public void Remove(ref RecyclableGUIComponent guiComponent, EGID egid)
        {
            if (guiComponent.instanceId > 0)
            {
                _builder.FreeInstance(guiComponent);
            }
        }

        readonly IGUIBuilder _builder;
    }
}