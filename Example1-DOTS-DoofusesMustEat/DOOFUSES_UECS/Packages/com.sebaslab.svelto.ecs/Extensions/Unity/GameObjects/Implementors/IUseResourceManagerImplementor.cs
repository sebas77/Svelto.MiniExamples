using Svelto.ECS.Hybrid;

namespace Svelto.ECS.Extensions.Unity
{
    public interface IUseResourceManagerImplementor: IImplementor

    {
    public IECSManager resourceManager { set; }
    }
}