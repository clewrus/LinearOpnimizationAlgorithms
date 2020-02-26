using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace YakimovTheSimplex.ViewModels {
	public class ViewModelCommand : ICommand {

		public ViewModelCommand (Action<object> executeAction) {
			this.executeAction = executeAction;
		}

		public ViewModelCommand (Action<object> executeAction, Predicate<object> canExecuteFunc) : this(executeAction) {
			this.canExecuteFunc = canExecuteFunc;
		}

		public bool CanExecute (object parameter) {
			if (canExecuteFunc != null) {
				return canExecuteFunc(parameter);
			}

			return true;
		}

		public void Execute (object parameter) {
			this.executeAction?.Invoke(parameter);
		}

		public event EventHandler CanExecuteChanged {
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		private readonly Action<object> executeAction;
		private Predicate<object> canExecuteFunc;
	}
}
