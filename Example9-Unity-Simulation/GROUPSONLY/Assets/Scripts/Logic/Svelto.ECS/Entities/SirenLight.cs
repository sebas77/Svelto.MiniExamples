using Svelto.ECS;

namespace Logic.SveltoECS
{
    /**
     * When this component is present on an entity, it's siren light should light up!
     */
    public struct SirenLight: IEntityComponent
    {
        public int LightIntensity;
    }
}