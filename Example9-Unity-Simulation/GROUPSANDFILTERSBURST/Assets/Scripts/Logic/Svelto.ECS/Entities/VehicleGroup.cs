using Svelto.ECS;

namespace Logic.SveltoECS
{
    public static class VechilesFilterIds
    {
        static readonly FilterContextID VehicleFilterContext = FilterContextID.GetNewContextID();
        
        public static CombinedFilterID VehiclesWithSirenOn = new CombinedFilterID(0, VehicleFilterContext);
        public static CombinedFilterID VehiclesWithSirenOff = new CombinedFilterID(1, VehicleFilterContext);
    }

    public sealed class VehicleTag: GroupTag<VehicleTag> 
    {
        static VehicleTag()
        {
            range = (ushort)Data.MaxTeamCount;
        }
    }
}
