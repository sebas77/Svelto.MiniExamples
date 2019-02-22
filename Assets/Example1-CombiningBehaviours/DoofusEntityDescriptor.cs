using Svelto.ECS.EntityStructs;

namespace Svelto.ECS.MiniExamples.Example1
{
    class DoofusEntityDescriptor
        : GenericEntityDescriptor<PositionEntityStruct, RotationEntityStruct, InterpolateVector3EntityStruct>
    {
    }
}