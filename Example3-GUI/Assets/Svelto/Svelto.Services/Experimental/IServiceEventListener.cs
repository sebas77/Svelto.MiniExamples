namespace Svelto.ServiceLayer.Experimental
{
    interface IServiceEventListener<TData1, TData2> : IServiceEventListenerBase
    {
    }

    interface IServiceEventListener<TData> : IServiceEventListenerBase
    {
    }

    interface IServiceEventListener : IServiceEventListenerBase
    {
    }

    // This interface exists so we can use one type which can represent any of the interfaces above
    interface IServiceEventListenerBase
    {
    }
}
