using Unity.Entities;

namespace Svelto.ECS.MiniExamples.DoofusesDOTS
{
    public struct PrefabsComponents: IComponentData
    {
        public Entity BlueDoofus;
        public Entity RedDoofus;
        public Entity SpecialDoofus;
        public Entity RedFood;
        public Entity BlueFood;
    }
}