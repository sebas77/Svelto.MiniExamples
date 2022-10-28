using Svelto.DataStructures;

namespace Svelto.ECS.Example.Survive
{
    /// <summary>
    /// If you are wondering why bothering with a decoupled way to sort the engines in the list, instead than
    /// just to use the engines as they are found in the list, you are correct to wonder.
    /// In this example it doesn't make much sense, but when you start to use encapsulated composition roots
    /// in packaged assemblies, this method allows to avoid circular dependencies. It's here just for
    /// demonstration purposes
    /// </summary>
    public class SortedEnginesGroup : SortedEnginesGroup<IStepEngine, SortedTickedEnginesOrder>
    {
        public SortedEnginesGroup(FasterList<IStepEngine> engines) : base(engines)
        {
        }
    }
}