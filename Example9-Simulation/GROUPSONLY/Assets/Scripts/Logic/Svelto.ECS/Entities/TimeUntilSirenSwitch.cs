// Copyright (c) Sean Nowotny

using Svelto.ECS;

namespace Logic.SveltoECS
{
    /**
     * The time until the siren light is to be turned on / off
     */
    public struct TimeUntilSirenSwitch: IEntityComponent
    {
        public float Value;
    }
}