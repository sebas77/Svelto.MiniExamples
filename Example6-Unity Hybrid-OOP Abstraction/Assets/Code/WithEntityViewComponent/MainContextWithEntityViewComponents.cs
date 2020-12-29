using Svelto.Context;
using UnityEngine;

namespace Svelto.ECS.Example.OOPAbstraction.EntityViewComponents
{
    public class MainContextWithEntityViewComponents : UnityContext<MainCompositionRootWithEntityViewComponents>
    {
        [TextArea] public string note =
            "Enable this context to check how oop can be wrapped around EntityViewComponents";
    }
}