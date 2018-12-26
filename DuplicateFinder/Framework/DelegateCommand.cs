using System;
using System.Windows.Input;

namespace DuplicateFinder.Framework
{
    // https://blogs.msdn.microsoft.com/msgulfcommunity/2013/03/13/understanding-the-basics-of-mvvm-design-pattern/
    public class DelegateCommand : ICommand
    {
        private readonly Action _command;

        public bool IsEnabled { get; set; }

        public DelegateCommand(Action command, bool isEnabled)
        {
            _command = command;
            IsEnabled = isEnabled;
        }

        public void Execute(object parameter)
        {
            _command();
        }

        public bool CanExecute(object parameter)
        {
            return IsEnabled;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        protected void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        // Helper method for consistant use with AsyncCommand
        public static DelegateCommand Create(Action command, bool isEnabled = true)
        {
            return new DelegateCommand(command, isEnabled);
        }
    }
}
