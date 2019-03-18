using Svelto.ECS.EntityStructs;
using UnityEngine;

namespace Svelto.ECS.MiniExamples.Example1
{
    public class FoodEntityDescriptor : GenericEntityDescriptor<PositionEntityStruct, UnityECSEntityStruct>
    {
    }
    
    class UnityECSFoodGroup:Component
    {
    }
}