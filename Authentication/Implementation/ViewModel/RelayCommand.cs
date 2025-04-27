using System;
using System.Windows.Input;

namespace DrinkDb_Auth.ViewModel
{
    public class RelayCommand : ICommand
    {
        private readonly Action executableActions;
        private readonly Func<bool>? isExecutable;

        public RelayCommand(Action actions, Func<bool>? executableChecker = null)
        {
            this.executableActions = actions;
            this.isExecutable = executableChecker;
        }

        public bool CanExecute(object? providedObject) => this.isExecutable == null || this.isExecutable();

        public void Execute(object? providedObject)
        {
            this.executableActions();
        }

        public event EventHandler? CanExecuteChanged;

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
