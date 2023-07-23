namespace Svelto.Command
{
    public interface ICommandFactory
    {
        TCommand Build<TCommand>() where TCommand : ICommand, new();
    }
}

