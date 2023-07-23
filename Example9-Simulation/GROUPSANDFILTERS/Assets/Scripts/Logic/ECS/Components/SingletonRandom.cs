// Copyright (c) Sean Nowotny

using Unity.Entities;
using Unity.Mathematics;

namespace Logic.ECS.Components
{
    public struct SingletonRandom: IComponentData
    {
        public Random Random;
    }
}