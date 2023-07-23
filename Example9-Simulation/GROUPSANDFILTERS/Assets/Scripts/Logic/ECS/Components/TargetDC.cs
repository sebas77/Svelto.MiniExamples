// Copyright (c) Sean Nowotny

using Unity.Entities;

namespace Logic.ECS.Components
{
    public partial struct TargetDC: IComponentData
    {
        public Entity Value;
    }
}