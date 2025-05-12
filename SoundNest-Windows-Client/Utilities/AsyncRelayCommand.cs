using Services.Infrestructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SoundNest_Windows_Client.Utilities
{
    public class AsyncRelayCommand : ICommand, IRelayCommand
    {
        private readonly Func<object, Task> executeAsync;
        private readonly Predicate<object> canExecute;
        private readonly Func<Task> executeNoParamAsync;
        private bool isExecuting;
        private event EventHandler CanExecuteChangedInternal;

        public AsyncRelayCommand(Func<object, Task> executeAsync, Predicate<object> canExecute = null)
        {
            this.executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
            this.canExecute = canExecute;
        }

        public AsyncRelayCommand(Func<Task> executeNoParamAsync)
            : this(o => executeNoParamAsync(), null)
        {
            this.executeNoParamAsync = executeNoParamAsync ?? throw new ArgumentNullException(nameof(executeNoParamAsync));
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
                CanExecuteChangedInternal += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
                CanExecuteChangedInternal -= value;
            }
        }

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return !isExecuting && (canExecute == null || canExecute(parameter));
        }

        public async void Execute(object parameter)
        {
            isExecuting = true;
            RaiseCanExecuteChanged();

            try
            {
                if (executeAsync != null)
                    await executeAsync(parameter);
                else if (executeNoParamAsync != null)
                    await executeNoParamAsync();
            }
            finally
            {
                isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChangedInternal?.Invoke(this, EventArgs.Empty);
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
