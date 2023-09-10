namespace Svelto.ECS.GUI
{
    public interface IGUIRecycler
    {
        void Recycle(RecyclableGUIComponent dynamicGUI);
    }
}