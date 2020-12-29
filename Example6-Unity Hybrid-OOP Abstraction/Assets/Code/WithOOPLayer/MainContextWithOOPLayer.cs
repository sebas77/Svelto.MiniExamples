using Svelto.Context;
using UnityEngine;

namespace Svelto.ECS.Example.OOPAbstraction.WithOOPLayer
{
    public class MainContextWithOOPLayer : UnityContext<MainCompositionRootWithOOPLayer>
    {
        [TextArea] public string note = "Enable this context to check how oop can be abstracted using code layers";
    }
}