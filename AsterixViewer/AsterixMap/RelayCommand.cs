using System.Windows.Input;

namespace AsterixViewer.AsterixMap
{
    public class RelayCommand(Action execute) : ICommand
    {
        public event EventHandler? CanExecuteChanged;
        private readonly Action _execute = execute;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            _execute();
        }
    }
}
