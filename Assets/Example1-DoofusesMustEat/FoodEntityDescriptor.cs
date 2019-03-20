using Svelto.ECS.EntityStructs;
using UnityEngine;

namespace Svelto.ECS.MiniExamples.Example1
{
    public class FoodEntityDescriptor : GenericEntityDescriptor<PositionEntityStruct, UnityECSEntityStruct, MealEntityStruct>
    {
    }
    
    class UnityECSFoodGroup:Component
    {
    }
}