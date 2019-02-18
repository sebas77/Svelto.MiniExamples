using Svelto.Context;
using UnityEngine;

namespace Svelto.ECS.MiniExamples.Example1
{
    public class SveltoContext : UnityContext<SveltoCompositionRoot>
    {
        public Mesh mesh;
        public Material material;
    }
}
