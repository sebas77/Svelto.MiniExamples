using Svelto.ECS.Hybrid;

namespace Svelto.ECS.Extensions.Unity
{
    public interface IUseMultipleResourceManagerImplementor: IImplementor

    {
    public IECSManager[] resourceManager { set; }
    }
}