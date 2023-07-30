using Svelto.ECS;

namespace Logic.SveltoECS
{
    public sealed class VehicleSirenOn: GroupCompound<VehicleTag, SirenTag> { }
    public sealed class VehicleSirenOff: GroupCompound<VehicleTag, WithoutSirenTag> { }
    
    public sealed class SirenTag: GroupTag<SirenTag> { }
    public sealed class WithoutSirenTag: GroupTag<WithoutSirenTag> { }
    
    public sealed class VehicleTag: GroupTag<VehicleTag> 
    {
        static VehicleTag()
        {
            range = (ushort)Data.MaxTeamCount;
        }
    }

    public static class ExclusiveGroups
    {
        public static ExclusiveGroup TimeGroup = new ExclusiveGroup();
    }
}