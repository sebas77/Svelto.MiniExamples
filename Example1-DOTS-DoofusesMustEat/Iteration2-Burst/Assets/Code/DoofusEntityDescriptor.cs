using Svelto.ECS.EntityStructs;

namespace Svelto.ECS.MiniExamples.Example1B
{
    class DoofusEntityDescriptor
        : GenericEntityDescriptor<PositionEntityStruct, UnityEcsEntityStructStruct, VelocityEntityStruct, HungerEntityStruct,
            SpeedEntityStruct>
    {
    }
}