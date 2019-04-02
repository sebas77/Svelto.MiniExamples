using Svelto.Context;
using UnityEngine;

namespace Svelto.ECS.MiniExamples.Example1C
{
    public class SveltoContext : UnityContext<SveltoCompositionRoot>
    {
        public GameObject capsule;
        public GameObject food;

        [TextArea] public string Notes;
    }
}
