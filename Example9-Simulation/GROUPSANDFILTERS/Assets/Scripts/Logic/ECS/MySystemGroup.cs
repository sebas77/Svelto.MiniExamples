// Copyright (c) Sean Nowotny

using Unity.Entities;
using Unity.Transforms;

namespace Logic.ECS
{
    [UpdateBefore(typeof(TransformSystemGroup))]
    public partial class MySystemGroup: ComponentSystemGroup
    {
    }
}