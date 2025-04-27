namespace App1.ViewModels
{
    using System;
    using System.Windows.Input;

    /// <summary>
    /// Represents a generic relay command that can be used to bind actions to UI events.
    /// </summary>
    /// <typeparam name="T">The type of the command parameter.</typeparam>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> execute;
        private readonly Predicate<T> canExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand{T}"/> class.
        /// </summary>
        /// <param name="execute">The action to execute when the command is invoked.</param>
        /// <param name="canExecute">An optional predicate to determine if the command can execute.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="execute"/> is null.</exception>
        public RelayCommand(Action<T> execute, Predicate<T>? canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        /// <summary>
        /// Occurs when changes occur that affect whether the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        /// <returns><c>true</c> if the command can execute; otherwise, <c>false</c>.</returns>
        public bool CanExecute(object parameter) => this.canExecute?.Invoke((T)parameter) ?? true;

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        public void Execute(object parameter) => this.execute((T)parameter);

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event to indicate that the return value of <see cref="CanExecute"/> might have changed.
        /// </summary>
        public void RaiseCanExecuteChanged() => this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
