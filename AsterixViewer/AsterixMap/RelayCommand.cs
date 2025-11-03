using System.Windows.Input;

namespace AsterixViewer.AsterixMap
{
    public class RelayCommand(Action<object?> execute) : ICommand
    {
        public event EventHandler? CanExecuteChanged;
        private readonly Action<object?> _execute = execute;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            _execute(parameter);
        }
    }
}
