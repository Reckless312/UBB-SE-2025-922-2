using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DrinkDb_Auth.ViewModel.AdminDashboard.Components
{
    public class RelayCommand : ICommand
    {
        private readonly Action executableActions;
        private readonly Func<bool>? isExecutable;

        public RelayCommand(Action actions, Func<bool>? executableChecker = null)
        {
            executableActions = actions;
            isExecutable = executableChecker;
        }

        public bool CanExecute(object? providedObject) => isExecutable == null || isExecutable();

        public void Execute(object? providedObject)
        {
            executableActions();
        }

        public event EventHandler? CanExecuteChanged;

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> execute;
        private readonly Func<bool>? canExecute;
        private bool isExecuting;

        public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => canExecute == null || canExecute();

        public async void Execute(object? parameter)
        {
            isExecuting = true;
            await execute();
            isExecuting = false;
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
