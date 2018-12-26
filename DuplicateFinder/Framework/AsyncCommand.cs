using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace DuplicateFinder.Framework
{
    // https://msdn.microsoft.com/en-us/magazine/msdnmag0414
    public class AsyncCommand<TResult> : AsyncCommandBase, INotifyPropertyChanged
    {
        private readonly Func<Task<TResult>> _command;
        private NotifyTaskCompletion<TResult> _execution;

        public bool IsEnabled { get; set; }

        public AsyncCommand(Func<Task<TResult>> command, bool isEnabled)
        {
            _command = command;
            IsEnabled = isEnabled;
        }

        public override bool CanExecute(object parameter)
        {
            return (Execution == null || Execution.IsCompleted) && IsEnabled;
        }

        public override async Task ExecuteAsync(object parameter)
        {
            Execution = new NotifyTaskCompletion<TResult>(_command());
            RaiseCanExecuteChanged();
            await Execution.TaskCompletion;
            RaiseCanExecuteChanged();
        }

        public NotifyTaskCompletion<TResult> Execution
        {
            get { return _execution; }
            private set
            {
                _execution = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public static class AsyncCommand
    {
        public static AsyncCommand<object> Create(Func<Task> command, bool isEnabled = true)
        {
            return new AsyncCommand<object>(async () => { await command(); return null; }, isEnabled);
        }

        public static AsyncCommand<TResult> Create<TResult>(Func<Task<TResult>> command, bool isEnabled = true)
        {
            return new AsyncCommand<TResult>(command, true);
        }
    }
}
