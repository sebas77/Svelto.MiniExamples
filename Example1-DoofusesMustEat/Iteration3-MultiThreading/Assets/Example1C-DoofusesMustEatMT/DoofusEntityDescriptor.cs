using Svelto.ECS.EntityStructs;

namespace Svelto.ECS.MiniExamples.Example1C
{
    class DoofusEntityDescriptor
        : GenericEntityDescriptor<PositionEntityStruct, UnityEcsEntityStructStruct, VelocityEntityStruct, HungerEntityStruct,
            SpeedEntityStruct>
    {
    }
}