using Svelto.Context;
using UnityEngine;

namespace Svelto.ECS.MiniExamples.Example1B
{
    public class SveltoContext : UnityContext<SveltoCompositionRoot>
    {
        [TextArea] public string Notes;
    }
}
