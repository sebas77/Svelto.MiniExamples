#if UNITY_5 || UNITY_5_3_OR_NEWER
namespace Svelto.Context
{
    public interface IUnityCompositionRoot: ICompositionRoot
    {
        void OnContextCreated(UnityContext contextHolder);
    }
}
#endif