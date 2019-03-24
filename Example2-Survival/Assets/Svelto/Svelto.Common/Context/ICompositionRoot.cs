namespace Svelto.Context
{
    public interface ICompositionRoot
    {
        void OnContextInitialized();
        void OnContextDestroyed();
        void OnContextCreated<T>(T contextHolder);
    }
}
