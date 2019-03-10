using Svelto.ECS.EntityStructs;
using UnityEngine;

namespace Svelto.ECS.MiniExamples.Example1
{
    class FoodEntityDescriptor : GenericEntityDescriptor<PositionEntityStruct, UnityECSEntityStruct>
    {
    }
    
    class UnityECSFoodGroup:Component
    {
    }
}