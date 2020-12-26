namespace Svelto.Command
{
    // Does not work with EventDispatcher - must use the non-generic IDispatchableCommand instead
    public interface IInjectableCommand<T> : ICommand
    {
        ICommand Inject(T dependency);
    }

    // Does not work with EventDispatcher - must use the non-generic IDispatchableCommand instead
    public interface IInjectableCommandWithStruct<T> : ICommand where T:struct 
    {
        ICommand Inject(ref T dependency);
    }
}
