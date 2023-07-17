using Svelto.Context;
using UnityEngine;

namespace Logic.SveltoECS
{
    public class SveltoContext: UnityContext<SirensCompositionRoot>
    {
        public GameObject prefab;
        public Material[] materials;
        public Material sirenLightMaterial;
    }
}