namespace Svelto.Context
{
    public interface ICompositionRoot
    {
        void OnContextInitialized<T>(T contextHolder);
        void OnContextDestroyed();
        void OnContextCreated<T>(T contextHolder);
    }
}
