using System;

namespace Svelto.Command
{
    public sealed class CommandFactory : ICommandFactory
    {
        public CommandFactory()
        {}

        public CommandFactory(Action<ICommand> onNewCommand)
        {
            _onNewCommand = onNewCommand;
        }

        public TCommand Build<TCommand>() where TCommand : ICommand, new()
        {
            TCommand command = new TCommand();            

            OnNewCommand(command);

            return command;

        }

        public TCommand Build<TCommand, T>(T dependency) where TCommand : ICommand, new()
        {
            TCommand command = (TCommand) Activator.CreateInstance(typeof(TCommand), dependency);

            OnNewCommand(command);

            return command;
        }

        void OnNewCommand(ICommand command)
        {
            if (_onNewCommand != null)
                _onNewCommand(command);
        }

        Action<ICommand>    _onNewCommand;
    }
}
