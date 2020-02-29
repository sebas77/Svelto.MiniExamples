using Svelto.ECS.EntityStructs;

namespace Svelto.ECS.MiniExamples.Example1B
{
    class DoofusEntityDescriptor
        : GenericEntityDescriptor<PositionEntityStruct, UnityEcsEntityStruct, VelocityEntityStruct, SpeedEntityStruct>
    {
    }
}