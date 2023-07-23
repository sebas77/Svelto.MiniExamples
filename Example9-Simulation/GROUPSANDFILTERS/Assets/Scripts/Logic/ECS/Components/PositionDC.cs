// Copyright (c) Sean Nowotny

using Unity.Entities;
using Unity.Mathematics;

namespace Logic.ECS.Components
{
    public partial struct PositionDC: IComponentData
    {
        public float2 Value;
    }
}