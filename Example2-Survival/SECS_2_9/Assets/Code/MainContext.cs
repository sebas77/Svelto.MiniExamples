using Svelto.Context;

namespace Svelto.ECS.Example.Survive
{
    /// <summary>
    ///     At least One GameObject containing a UnityContext must be present in the scene.
    ///     All the monobehaviours existing in gameobjects child of the UnityContext one,
    ///     can be later queried, usually to create entities from statically created
    ///     gameobjects.
    /// </summary>
    public class MainContext : UnityContext<MainCompositionRoot>
    {
    }
}