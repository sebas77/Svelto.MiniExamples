namespace Svelto.ECS.MiniExamples.Example1
{
    static class GameGroups
    {
        public static readonly ExclusiveGroup DOOFUSESJUMPING = new ExclusiveGroup();
        public static readonly ExclusiveGroup DOOFUSESMOVING = new ExclusiveGroup();

        public static readonly ExclusiveGroup[] DOOFUSES = {DOOFUSESMOVING, DOOFUSESJUMPING};
    }
}