using System;
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
}
